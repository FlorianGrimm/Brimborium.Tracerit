import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { AsyncPipe } from '@angular/common';
import type { LogFileInformationList, LogFileInformation } from '../Api';

@Component({
  selector: 'app-directory-list',
  imports: [AsyncPipe],
  templateUrl: './directory-list.component.html',
  styleUrl: './directory-list.component.less'
})
export class DirectoryListComponent implements OnInit, OnDestroy {
  subscription = new Subscription();
  dataService = inject(DataService);
  httpClientSerive = inject(HttpClientService);

  directoryList$ = new BehaviorSubject<LogFileInformationList>([]);
  error$ = new BehaviorSubject<undefined | string>(undefined);

  constructor() {
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  load() {
    this.httpClientSerive.getDirectoryList().subscribe({
      next: (value) => {

        console.log("value", value);
        if ('success' === value.mode) {
          this.directoryList$.next(value.files);
          this.error$.next(undefined);
        } else if ("error" === value.mode) {
          this.directoryList$.next([]);
          this.error$.next(value.error);
        }

      },
      complete: () => console.log('complete'),
      error: (error) => console.error(error)
    });
  }
}
