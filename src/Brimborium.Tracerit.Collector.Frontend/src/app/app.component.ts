import { Component, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Home, FileStack, ChevronLeft, ChevronRight } from 'lucide-angular';
import { DataService } from './Utility/data-service';
import { HttpClientService } from './Utility/http-client-service';
import { take } from 'rxjs';

@Component({
    selector: 'app-root',
    imports: [
        FormsModule,
        RouterOutlet,
        RouterLink,
        //RouterLinkActive,
        LucideAngularModule

    ],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent {
    readonly httpClientService = inject(HttpClientService);
    readonly dataService = inject(DataService);

    readonly title = 'Tracerit';
    readonly Home = Home;
    readonly ChevronLeft = ChevronLeft;
    readonly FileStack = FileStack;
    readonly ChevronRight = ChevronRight;

    protected expanded = signal(false);
    protected open = false;
    protected switch = false;


    constructor() {
        window.addEventListener('focus', (event: FocusEvent) => { this.onWindowFocus(event); });
    }

    onWindowFocus(focusEvent: FocusEvent) {
        if (((focusEvent.target as any)?.__proto__) !== Window.prototype) { return; }
        if (this.dataService.useCurrentStream$.getValue()) {
            this.httpClientService.getCurrentStream()
                .pipe(take(1))
                .subscribe({
                    next: (value) => {
                        if ("success" === value.mode) {
                            this.dataService.setListLogLine(value.data);
                        }
                    },
                });
        }
    }

    protected handleToggle(): void {
        this.expanded.update((e) => !e);
    }

}
