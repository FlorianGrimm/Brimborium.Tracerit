import { Component, signal, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DataService } from '@app/Utility/data-service';
import { HttpClientService } from '@app/Utility/http-client-service';
import { distinctUntilChanged, filter, repeat, Subscription, switchMap, take, tap } from 'rxjs';
import { BehaviorRingSubject, createBehaviorRingSubject } from '@app/Utility/BehaviorRingSubject';
import { createObserableSubscripe } from '@app/Utility/ObservableSubscripe';
import { combineLatestSubject } from '@app/Utility/CombineLatestSubject';
import { MasterRingService } from '@app/Utility/master-ring.service';
import { LucideAngularModule } from 'lucide-angular';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
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

    readonly subscription = new Subscription();
    readonly router = inject(Router);

    readonly ring$ = inject(MasterRingService).dependendRing('LogTimeDataService-ring$', this.subscription);

    readonly icon = new AppIconComponent();
    readonly title = 'Tracerit';
    protected expanded = signal(false);
    protected open = false;
    protected switch = false;


    readonly visibilityState$ = createBehaviorRingSubject<string>({
        subscription: this.subscription,
        initialValue: document.visibilityState,
        ring: 1,
        conditionSubject: this.ring$,
        name: 'visibilityState$',
        fnReport: (name, message, value) => { console.log(name, message, value); }
    });
    readonly triggerReload$ = combineLatestSubject({
        dictObservable: {
        visibilityState: this.visibilityState$,
        useCurrentStream: this.dataService.useCurrentStream$.pipe(distinctUntilChanged())
        },
        name: 'visibilityState$'
    });
    readonly reloadCurrentStream$=createObserableSubscripe({
        obs: this.triggerReload$.combineLatest().pipe(
          filter(value => value.visibilityState === 'visible' && value.useCurrentStream),
          switchMap(() => this.httpClientService.getCurrentStream(this.dataService.currentStreamName).pipe(take(1))),
          tap({
            next: (value) => {
                console.log("reloadCurrentStream$", {mode:value.mode, data:value.data?.length});
                if ("success" === value.mode) {
                    this.dataService.addListLogLine(value.data);
                }
            }
          }),
          repeat()
        ),
        subscribtion: this.subscription,
        immediate: true
      });

    constructor() {
        /*
        window.addEventListener('focus', (currentEvent: FocusEvent) => {
            if (((currentEvent.target as any)?.__proto__) !== Window.prototype) { return; }
            this.reloadIfNecessary();
        });
        */
        window.addEventListener('visibilitychange', (currentEvent: Event) => {
            this.visibilityState$.next(document.visibilityState);
        });
        this.router.events.subscribe((event) => {
            if (event instanceof NavigationEnd) {
                console.log("NavigationEnd", event);
            }
        });
    }

    protected handleToggle(): void {
        this.expanded.update((e) => !e);
    }

}
