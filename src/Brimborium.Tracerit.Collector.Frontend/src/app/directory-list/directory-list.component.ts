import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, tap, debounceTime, Subscription, forkJoin, map, take } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { AsyncPipe } from '@angular/common';
import { FileSizePipe } from '../pipe/file-size.pipe';

import type { LogFileInformationList, LogFileInformation, LogLine } from '../Api';
import { Router } from '@angular/router';
import { BehaviorRingSubject } from '../Utility/BehaviorRingSubject';
import { MasterRingSubject } from "../Utility/MasterRingSubject";
import { MasterRingService } from '../Utility/master-ring.service';
import { AppRingOrder } from '../app-ring-order';

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

  currentFile$ = new BehaviorRingSubject<string | undefined>(undefined, 1, 'currentFile$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  listFile$ = new BehaviorRingSubject<LogFileInformationList>([], 1, 'listFile$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  error$ = new BehaviorRingSubject<undefined | string>(undefined, 1, 'error$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  selected$ = new BehaviorRingSubject<string[]>([], 
    AppRingOrder.DirectoryListComponent_selected, 'DirectoryListComponent_selected', this.subscription, this.ring$, undefined, 
    (name, message, value) => { console.log(name, message, value); });
  
  loaded$ = new BehaviorRingSubject<boolean>(false, 
    AppRingOrder.DirectoryListComponent_loaded, 'DirectoryListComponent_loaded', this.subscription, this.ring$, undefined, 
    (name, message, value) => { console.log(name, message, value); });
  listFileLoading$ = new BehaviorRingSubject<string[]>([], 1, 'listFileLoading$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  constructor() {
    this.subscription.add(this.dataService.listFile$.subscribe(this.listFile$));
    this.subscription.add(this.dataService.currentFile$.subscribe(this.currentFile$));
    this.subscription.add(this.dataService.listSelectedFileName$.subscribe(this.selected$));
  }

  ngOnInit(): void {
    this.load();

    this.subscription.add(
      combineLatest({
        loaded: this.loaded$,
        listSelectedFileName: this.dataService.listSelectedFileName$,
        selected: this.selected$,
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
          this.error$.next(error.toString());
        }
      }));
  }

  loadFile(name: string) {
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
            this.error$.next(err.toString());
          }
        }));
    return false;
  }

  onSelectedChange(name: string) {
    const selected = this.selected$.getValue()
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
              this.error$.next(err.toString());
              this.listFileLoading$.next(this.listFileLoading$.getValue().filter(item => item != name));
            }
          }));
    }
    this.selected$.next(nextSelected);

    return false;
  }

}
