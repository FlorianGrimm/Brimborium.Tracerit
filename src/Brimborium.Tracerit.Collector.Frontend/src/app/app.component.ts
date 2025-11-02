import { Component, signal ,ChangeDetectionStrategy} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive} from '@angular/router';
import { LucideAngularModule, Home, FileStack, ChevronLeft, ChevronRight } from 'lucide-angular';

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
    readonly title = 'Tracerit';
    readonly Home = Home;
    readonly ChevronLeft = ChevronLeft;
    readonly FileStack=FileStack;
    readonly ChevronRight = ChevronRight;

	protected expanded = signal(false);
    protected open = false;
    protected switch = false;
 
    protected handleToggle(): void {
        this.expanded.update((e) => !e);
    }

}
