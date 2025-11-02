import { Component, inject } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { LogLine, PropertyHeader } from '../Api';
import { AsyncPipe } from '@angular/common';
import { getVisualHeader } from '../Utility/propertyHeaderUtility';

@Component({
  selector: 'app-log-view',
  imports: [AsyncPipe],
  templateUrl: './log-view.component.html',
  styleUrl: './log-view.component.scss',
})
export class LogViewComponent {
  subscription = new Subscription();
  dataService = inject(DataService);
  httpClientService = inject(HttpClientService);
  listAllHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  listCurrentHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  currentLogLine$ = new BehaviorSubject<number | undefined>(undefined);
  error$ = new BehaviorSubject<undefined | string>(undefined);

  constructor() {
    this.subscription.add(
      this.dataService.listLogLine$.subscribe(this.listLogLine$));
    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          this.listAllHeader$.next(value);
          this.listCurrentHeader$.next(getVisualHeader(value));
        }
      }));
  }

  setCurrentLogLine(id: number) {
    this.currentLogLine$.next(id);
  }
}
