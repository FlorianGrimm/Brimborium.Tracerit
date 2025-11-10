import { TestBed } from '@angular/core/testing';

import { LogTimeDataService } from './log-time-data.service';

describe('LogTimeDataService', () => {
  let service: LogTimeDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LogTimeDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
