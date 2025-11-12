import { TestBed } from '@angular/core/testing';

import { MasterRingService } from './master-ring.service';

describe('MasterRingService', () => {
  let service: MasterRingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MasterRingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
