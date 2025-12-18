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

    // TODO
    // readonly reloadCurrentStream = this.depDataService.createProperty({
    //     name: 'AppComponent_triggerReload',
    //     initialValue: 0,
    //     subscription: this.subscription,
    // }).withSource({
    //     sourceDependency: {
    //         visibilityState: this.visibilityState.dependencyInner(),
    //         useCurrentStream: this.dataService.useCurrentStream.dependencyInner(),
    //     },
    //     sourceTransform:
    //         (d) => {
    //             if (d.visibilityState === 'visible' && d.useCurrentStream) {
    //                 return 1;
    //             }
    //             return 2;
    //         },
    //     depDataPropertyInitializer: this.depDataPropertyInitializer
    // });

    // readonly triggerReload$ = combineLatestSubject({
    //     dictObservable: {
    //     visibilityState: this.visibilityState$,
    //     useCurrentStream: this.dataService.useCurrentStream$.pipe(distinctUntilChanged())
    //     },
    //     name: 'visibilityState$'
    // });
    // readonly reloadCurrentStream$=createObserableSubscripe({
    //     obs: this.triggerReload$.combineLatest().pipe(
    //       filter(value => value.visibilityState === 'visible' && value.useCurrentStream),
    //       switchMap(() => this.httpClientService.getCurrentStream(this.dataService.currentStreamName).pipe(take(1))),
    //       tap({
    //         next: (value) => {
    //             console.log("reloadCurrentStream$", {mode:value.mode, data:value.data?.length});
    //             if ("success" === value.mode) {
    //                 //this.dataService.addListLogLine(value.data);
    //             }
    //         }
    //       }),
    //       repeat()
    //     ),
    //     subscribtion: this.subscription,
    //     immediate: true
    //   });

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
