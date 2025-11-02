import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { AsyncPipe } from '@angular/common';
import { FileSizePipe } from '../pipe/file-size.pipe';

import type { LogFileInformationList, LogFileInformation } from '../Api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-directory-list',
  imports: [AsyncPipe, FileSizePipe],
  templateUrl: './directory-list.component.html',
  styleUrl: './directory-list.component.less'
})
export class DirectoryListComponent implements OnInit, OnDestroy {
  subscription = new Subscription();
  router = inject(Router);
  dataService = inject(DataService);
  httpClientService = inject(HttpClientService);

  currentFile$ = new BehaviorSubject<string | undefined>(undefined);
  listFile$ = new BehaviorSubject<LogFileInformationList>([]);
  error$ = new BehaviorSubject<undefined | string>(undefined);


  constructor() {
    this.subscription.add(this.dataService.listFile$.subscribe(this.listFile$));
    this.subscription.add(this.dataService.currentFile$.subscribe(this.currentFile$));
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  load() {
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getDirectoryList().subscribe({
        next: (value) => {
          if ('success' === value.mode) {
            this.dataService.setListFile(value.files);
            this.error$.next(undefined);
          } else if ("error" === value.mode) {
            this.listFile$.next([]);
            this.error$.next(value.error);
          }
        },
        complete: () => {
          subscription.unsubscribe();
          console.log('complete');
        },
        error: (error) => {
          this.listFile$.next([]);
          this.error$.next(error.toString());
        }
      }));
  }

  setCurrentFils(name: string) {
    const directoryList = this.listFile$.getValue();
    const listMatch = directoryList.filter(item => name === item.name);
    if (1 != listMatch.length) { return false; }

    const currentFile = listMatch[0];
    this.dataService.setCurrentFile(currentFile.name);

    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getFile(currentFile.name)
        .subscribe({
          next: (value) => {
            if ("success" === value.mode) {
              this.dataService.setListLogLine(value.data);
              this.router.navigate(['tracorit', 'log']);
            } else {
              this.error$.next(value.error);
            }
          },
          complete: () => {
          },
          error: (err) => {
            this.error$.next(err.toString());
          }
        }));
    return false;
  }
}
