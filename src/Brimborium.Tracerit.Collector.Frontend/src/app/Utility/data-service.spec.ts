import { TestBed } from '@angular/core/testing';

import { DataService } from './data-service';
import { provideZonelessChangeDetection } from '@angular/core';

describe('DataService', () => {
  let service: DataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
    service = TestBed.inject(DataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
