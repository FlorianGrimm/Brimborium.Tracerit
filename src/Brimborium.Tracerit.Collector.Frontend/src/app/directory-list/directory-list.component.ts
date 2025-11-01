import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { LogFileInformationList } from '../Api';

@Component({
  selector: 'app-directory-list',
  imports: [],
  templateUrl: './directory-list.component.html',
  styleUrl: './directory-list.component.scss'
})
export class DirectoryListComponent implements OnInit, OnDestroy {
  subscription = new Subscription();
  dataService = inject(DataService);
  httpClientSerive = inject(HttpClientService);

  directoryList$ = new BehaviorSubject<LogFileInformationList>([]);

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
      next: (value) => this.directoryList$.next(value),
      complete: () => console.log('complete'),
      error: (error) => console.error(error)
    });
  }
}
