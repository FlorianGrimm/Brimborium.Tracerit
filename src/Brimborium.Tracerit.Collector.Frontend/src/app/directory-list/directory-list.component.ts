import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, debounceTime, Subscription, map, take } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { AsyncPipe } from '@angular/common';
import { FileSizePipe } from '../pipe/file-size.pipe';

import type { LogFileInformationList, LogLine } from '../Api';
import { Router } from '@angular/router';
import { BehaviorRingSubject } from '../Utility/BehaviorRingSubject';
import { MasterRingService } from '../Utility/master-ring.service';

@Component({
  selector: 'app-directory-list',
  imports: [AsyncPipe, FileSizePipe],
  templateUrl: './directory-list.component.html',
  styleUrl: './directory-list.component.scss'
})
export class DirectoryListComponent implements OnInit, OnDestroy {
  subscription = new Subscription();

  ring$ = inject(MasterRingService).dependendRing('DirectoryListComponent-ring$', this.subscription);
  router = inject(Router);

  dataService = inject(DataService);
  httpClientService = inject(HttpClientService);

  currentFile$ = new BehaviorRingSubject<string | undefined>(undefined, 1, 'DirectoryListComponent_currentFile$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  listFile$ = new BehaviorRingSubject<LogFileInformationList>([], 1, 'DirectoryListComponent_listFile$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  error$ = new BehaviorRingSubject<undefined | string | object>(undefined, 1, 'DirectoryListComponent_error$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  listSelectedFileName$ = new BehaviorRingSubject<string[]>([],
    0, 'DirectoryListComponent_listSelectedFileName$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  loaded$ = new BehaviorRingSubject<boolean>(false,
    0, 'DirectoryListComponent_loaded', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  listFileLoading$ = new BehaviorRingSubject<string[]>([], 1, 'listFileLoading$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  constructor() {
    this.subscription.add(this.dataService.listFile$.subscribe(this.listFile$));
    this.subscription.add(this.dataService.currentFile$.subscribe(this.currentFile$));
    this.subscription.add(this.dataService.listSelectedFileName$.subscribe(this.listSelectedFileName$));
  }

  ngOnInit(): void {
    this.load();

    this.subscription.add(
      combineLatest({
        loaded: this.loaded$,
        listSelectedFileName: this.dataService.listSelectedFileName$,
        selected: this.listSelectedFileName$,
        listFile: this.dataService.listFile$
      }).pipe(
        filter((value) => value.loaded),
        filter((value) => (0 < value.listFile.length)),
        debounceTime(3000),
        filter((value) => (0 < value.selected.length)),
        filter((value) => (value.listSelectedFileName != value.selected))
      ).subscribe({
        next: (value) => {
          const listSelectedLogLines: BehaviorSubject<LogLine[]>[] = [];
          const listSelectedName: string[] = [];

          for (let name of value.selected) {
            const listLogLine$ = this.dataService.mapLogLineByName.get(name);
            if (undefined === listLogLine$) { continue; }
            listSelectedLogLines.push(listLogLine$);
            listSelectedName.push(name);
          }

          const subscription = new Subscription();
          this.subscription.add(subscription);
          subscription.add(
            combineLatest(
              {
                condition: this.listFileLoading$.pipe(filter(item => 0 === item.length)),
                listSelectedLogLines: combineLatest(listSelectedLogLines)
              }
            ).pipe(
              take(1),
              map(value => value.listSelectedLogLines)
            ).subscribe({
              next: (value) => {
                this.dataService.clearMapLogLineByNameOthers(listSelectedName);
                const result = value.flat(1);
                this.dataService.setListLogLine(result);
              },
              complete: () => {
                this.router.navigate(['tracorit', 'log']);
              },
              error: () => {
                subscription.unsubscribe()
              }
            })
          );
        }
      })

    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  load() {
    this.dataService.useCurrentStream$.next(false);
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getDirectoryList().subscribe({
        next: (value) => {
          if ('success' === value.mode) {
            this.loaded$.next(true);            
            this.dataService.setListFile(value.files);
            this.error$.next(undefined);
          } else if ("error" === value.mode) {
            this.listFile$.next([]);
            this.error$.next(value.error);
          }
        },
        complete: () => {
          subscription.unsubscribe();
        },
        error: (error) => {
          this.listFile$.next([]);
          this.error$.next(error);
        }
      }));
  }

  loadCurrentStream() {
    this.dataService.useCurrentStream$.next(true);
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getCurrentStream(this.dataService.currentStreamName).subscribe({
        next: (value) => {
          if ("success" === value.mode) {
            this.dataService.addListLogLine(value.data);
            this.router.navigate(['tracorit', 'log']);
          } else {
            this.error$.next(value.error);
          }
        },
      }));
    return false;
  }

  loadFile(name: string) {
    this.dataService.useCurrentStream$.next(false);
    const directoryList = this.listFile$.getValue();
    const listMatch = directoryList.filter(item => name === item.name);
    if (1 != listMatch.length) { return false; }

    //const currentFile = listMatch[0];
    this.dataService.setCurrentFile(name);
    this.dataService.listSelectedFileName$.next([name]);

    this.loaded$.next(false);
    this.dataService.setListLogLineByName(name, []);
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getFile(name)
        .subscribe({
          next: (value) => {
            if ("success" === value.mode) {
              this.dataService.setListLogLineByName(name, value.data);
              this.dataService.setListLogLine(value.data);
              this.router.navigate(['tracorit', 'log']);
            } else {
              this.error$.next(value.error);
            }
          },
          complete: () => {
          },
          error: (err) => {
            this.error$.next(err);
          }
        }));
    return false;
  }

  onListSelectedFileNameChange(name: string) {
    const selected = this.listSelectedFileName$.getValue()
    let nextSelected: string[] = [];
    if (selected.includes(name)) {
      nextSelected = selected.filter(item => name != item);
    } else {
      nextSelected = [...selected, name];
      nextSelected.sort((a, b) => a.localeCompare(b));

      this.listFileLoading$.next([...this.listFileLoading$.getValue(), name]);
      this.dataService.setListLogLineByName(name, []);
      const subscription = new Subscription();
      this.subscription.add(subscription);
      subscription.add(
        this.httpClientService.getFile(name)
          .subscribe({
            next: (value) => {
              if ("success" === value.mode) {
                this.dataService.setListLogLineByName(name, value.data);
              } else {
                this.error$.next(value.error);
              }
            },
            complete: () => {
              this.listFileLoading$.next(this.listFileLoading$.getValue().filter(item => item != name));
            },
            error: (err) => {
              this.error$.next(err);
              this.listFileLoading$.next(this.listFileLoading$.getValue().filter(item => item != name));
            }
          }));
    }
    this.listSelectedFileName$.next(nextSelected);

    return false;
  }

  getErrorMessage(value: undefined | string | object | Error): string {
    if (value instanceof Error) {
      return value.message;
    }
    if (typeof value === 'string') { return value; }
    if (value == null) { return ""; }

    return JSON.stringify(value);
  }
}
