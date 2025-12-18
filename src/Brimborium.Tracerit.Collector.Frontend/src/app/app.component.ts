import { Component, signal, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { DataService } from '@app/Utility/data-service';
import { HttpClientService } from '@app/Utility/http-client-service';
import { Subscription } from 'rxjs';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
import { DepDataService } from './Utility/dep-data.service';
type ReloadCurrentStream = { trigger: boolean, tick: number };

@Component({
    selector: 'app-root',
    imports: [
        FormsModule,
        RouterOutlet,
        RouterLink,
        LucideAngularModule
    ],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent {
    readonly httpClientService = inject(HttpClientService);
    readonly dataService = inject(DataService);
    readonly depDataService = inject(DepDataService);
    readonly depDataPropertyInitializer = this.depDataService.createInitializer();

    readonly subscription = new Subscription();
    readonly router = inject(Router);

    readonly icon = new AppIconComponent();
    readonly title = 'Tracerit';
    protected expanded = signal(false);
    protected open = false;
    protected switch = false;

    readonly visibilityState = this.depDataService.createProperty({
        name: 'AppComponent_visibilityState',
        initialValue: document.visibilityState,
        subscription: this.subscription,
    });


    readonly reloadCurrentStream = this.depDataService.createProperty<ReloadCurrentStream>({
        name: 'AppComponent_triggerReload',
        initialValue: { trigger: false, tick: 0 },
        compare: (a, b) => a.tick === b.tick,
        sideEffect: {
          fn: (value) => {
            if (value.trigger) {
              this.httpClientService.getCurrentStream(this.dataService.currentStreamName).subscribe({
                next: (value) => {
                    // console.log("reloadCurrentStream$", {mode:value.mode, data:value.data?.length});
                    if ("success" === value.mode) {
                        this.dataService.addListLogLine(value.data);
                    }
                }
              });
            }
          },
          kind: 'UI',
          requestAnimationFrame: true,
        },
        subscription: this.subscription,
    }).withSource({
        sourceDependency: {
            visibilityState: this.visibilityState.dependencyInner(),
            useCurrentStream: this.dataService.useCurrentStream.dependencyInner(),
        },
        sourceTransform:
            ({visibilityState, useCurrentStream}) => {
                const currentValue = this.reloadCurrentStream.getValue();
                if (visibilityState === 'visible' && useCurrentStream) {
                    const result: ReloadCurrentStream = { trigger: true, tick: currentValue.tick + 1 };
                    return result;
                }
                {
                    const result: ReloadCurrentStream = { trigger: false, tick: currentValue.tick };
                    return result;;
                }
            },
        depDataPropertyInitializer: this.depDataPropertyInitializer
    });

    constructor() {
        this.depDataPropertyInitializer.execute();
        window.addEventListener('visibilitychange', (currentEvent: Event) => {
            this.visibilityState.setValue(document.visibilityState);
        });
        // TODO
        // this.router.events.subscribe((event) => {
        //     if (event instanceof NavigationEnd) {
        //         console.log("NavigationEnd", event);
        //     }
        // });
    }

    protected handleToggle(): void {
        this.expanded.update((e) => !e);
    }
}
