import { TestBed } from '@angular/core/testing';

import { TraceableSubjectGraphService } from './traceable-subject-graph.service';

describe('TraceableSubjectGraphService', () => {
  let service: TraceableSubjectGraphService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TraceableSubjectGraphService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
