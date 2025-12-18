import { TestBed } from '@angular/core/testing';

import { TraceableSubjectGraphService } from './traceable-subject-graph.service';
import { provideZonelessChangeDetection } from '@angular/core';

describe('TraceableSubjectGraphService', () => {
  let service: TraceableSubjectGraphService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
    service = TestBed.inject(TraceableSubjectGraphService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
