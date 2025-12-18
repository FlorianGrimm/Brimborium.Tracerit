import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, debounceTime, Subscription, map, take } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { FileSizePipe } from '../pipe/file-size.pipe';

import type { LogFileInformationList, LogLine } from '../Api';
import { Router } from '@angular/router';
import { DepDataService } from '@app/Utility/dep-data.service';

@Component({
  selector: 'app-directory-list',
  imports: [FileSizePipe],
  templateUrl: './directory-list.component.html',
  styleUrl: './directory-list.component.scss'
})
export class DirectoryListComponent implements OnInit, OnDestroy {
  readonly subscription = new Subscription();

  readonly router = inject(Router);
  readonly depDataService = inject(DepDataService);
  readonly depDataPropertyInitializer = this.depDataService.createInitializer();

  readonly dataService = inject(DataService);
  readonly httpClientService = inject(HttpClientService);

  readonly currentFile = this.depDataService.createProperty({
    name: 'DirectoryListComponent_currentFile',
    initialValue: undefined as (string | undefined),
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency:
      {
        currentFile: this.dataService.currentFile.dependencyInner()
      },
      sourceTransform:
        (d) => d.currentFile,
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );
  readonly $currentFile = this.currentFile.asSignal();

  readonly listFile = this.depDataService.createProperty({
    name: 'DirectoryListComponent_listFile',
    initialValue: [] as LogFileInformationList,
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency:
      {
        listFile: this.dataService.listFile.dependencyPublic()
      },
      sourceTransform:
        (d) => {
          return d.listFile;
        },
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );
  $listFile = this.listFile.asSignal();

  readonly error = this.depDataService.createProperty({
    name: 'DirectoryListComponent_error',
    initialValue: undefined as (undefined | string | object),
    subscription: this.subscription,
  });
  readonly $error = this.error.asSignal();

  readonly listSelectedFileName = this.depDataService.createProperty({
    name: 'DirectoryListComponent_listSelectedFileName',
    initialValue: [] as string[],
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency:
      {
        listSelectedFileName: this.dataService.listSelectedFileName.dependencyPublic()
      },
      sourceTransform:
        (d) => d.listSelectedFileName,
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );
  readonly $listSelectedFileName = this.listSelectedFileName.asSignal();
  readonly listSelectedFileName$ = this.listSelectedFileName.asObserable();

  readonly loaded = this.depDataService.createProperty<boolean>({
    name: 'DirectoryListComponent_loaded',
    initialValue: false,
    subscription: this.subscription,
  });

  readonly listFileLoading = this.depDataService.createProperty({
    name: 'DirectoryListComponent_listFileLoading',
    initialValue: [] as string[],
    subscription: this.subscription,
  });

  constructor() {
    this.depDataPropertyInitializer.execute();
  }

  ngOnInit(): void {
    this.load();

    this.subscription.add(
      combineLatest({
        loaded: this.loaded.asObserable(),
        listSelectedFileName: this.listSelectedFileName$,
        selected: this.listSelectedFileName.asObserable(),
        listFile: this.dataService.listFile.asObserable()
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
                condition: this.listFileLoading.asObserable().pipe(filter(item => 0 === item.length)),
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
    this.dataService.useCurrentStream.setValue(false);
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getDirectoryList().subscribe({
        next: (value) => {
          if ('success' === value.mode) {
            this.loaded.setValue(true);
            this.dataService.setListFile(value.files);
            this.error.setValue(undefined);
          } else if ("error" === value.mode) {
            this.listFile.setValue([]);
            this.error.setValue(value.error);
          }
        },
        complete: () => {
          subscription.unsubscribe();
        },
        error: (error) => {
          this.listFile.setValue([]);
          this.error.setValue(error);
        }
      }));
  }

  loadCurrentStream() {
    this.dataService.useCurrentStream.setValue(true);
    const subscription = new Subscription();
    this.subscription.add(subscription);
    subscription.add(
      this.httpClientService.getCurrentStream(this.dataService.currentStreamName).subscribe({
        next: (value) => {
          if ("success" === value.mode) {
            this.dataService.addListLogLine(value.data);
            this.router.navigate(['tracorit', 'log']);
          } else {
            this.error.setValue(value.error);
          }
        },
      }));
    return false;
  }

  loadFile(name: string) {
    this.dataService.useCurrentStream.setValue(false);
    const directoryList = this.listFile.getValue();
    const listMatch = directoryList.filter(item => name === item.name);
    if (1 != listMatch.length) { return false; }

    //const currentFile = listMatch[0];
    this.dataService.setCurrentFile(name);
    this.dataService.listSelectedFileName.setValue([name]);

    this.loaded.setValue(false);
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
              this.error.setValue(value.error);
            }
          },
          complete: () => {
          },
          error: (err) => {
            this.error.setValue(err);
          }
        }));
    return false;
  }

  onListSelectedFileNameChange(name: string) {
    const selected = this.listSelectedFileName.getValue()
    let nextSelected: string[] = [];
    if (selected.includes(name)) {
      nextSelected = selected.filter(item => name != item);
    } else {
      nextSelected = [...selected, name];
      nextSelected.sort((a, b) => a.localeCompare(b));

      this.listFileLoading.setValue([...this.listFileLoading.getValue(), name]);
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
                this.error.setValue(value.error);
              }
            },
            complete: () => {
              this.listFileLoading.setValue(this.listFileLoading.getValue().filter(item => item != name));
            },
            error: (err) => {
              this.error.setValue(err);
              this.listFileLoading.setValue(this.listFileLoading.getValue().filter(item => item != name));
            }
          }));
    }
    this.listSelectedFileName.setValue(nextSelected);

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
