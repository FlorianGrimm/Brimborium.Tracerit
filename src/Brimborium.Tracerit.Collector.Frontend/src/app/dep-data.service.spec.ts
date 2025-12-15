import { TestBed } from '@angular/core/testing';

import { DepDataService } from './dep-data.service';

describe('DepDataService', () => {
  let service: DepDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DepDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
