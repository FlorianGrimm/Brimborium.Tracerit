import { TestBed } from '@angular/core/testing';

import { LogTimeDataService } from './log-time-data.service';
import { provideZonelessChangeDetection } from '@angular/core';

describe('LogTimeDataService', () => {
  let service: LogTimeDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
    service = TestBed.inject(LogTimeDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
