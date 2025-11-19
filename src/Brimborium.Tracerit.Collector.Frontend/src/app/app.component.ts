import { Component, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Home, FileStack, ChevronLeft, ChevronRight } from 'lucide-angular';
import { DataService } from './Utility/data-service';
import { HttpClientService } from './Utility/http-client-service';
import { distinctUntilChanged, filter, repeat, Subscription, switchMap, take, tap } from 'rxjs';
import { BehaviorRingSubject, createBehaviorRingSubject } from './Utility/BehaviorRingSubject';
import { createObserableSubscripe } from './Utility/ObservableSubscripe';
import { combineLatestSubject } from './Utility/CombineLatestSubject';
import { MasterRingService } from './Utility/master-ring.service';

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
    readonly ring$ = inject(MasterRingService).dependendRing('LogTimeDataService-ring$', this.subscription);

    readonly title = 'Tracerit';
    readonly Home = Home;
    readonly ChevronLeft = ChevronLeft;
    readonly FileStack = FileStack;
    readonly ChevronRight = ChevronRight;

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
    }

    protected handleToggle(): void {
        this.expanded.update((e) => !e);
    }

}
