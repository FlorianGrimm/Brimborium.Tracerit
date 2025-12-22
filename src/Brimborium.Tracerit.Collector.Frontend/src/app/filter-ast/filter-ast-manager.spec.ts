import { Subscription } from 'rxjs';
import { FilterAstManager } from './filter-ast-manager';
import { DataService } from "@app/Utility/data-service";
import { TestBed } from '@angular/core/testing';

describe('FilterAstManager', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [DataService] });
  });

  it('should create an instance', () => {
    const subscription = new Subscription();
    let filterAstManager: FilterAstManager | undefined = undefined;;
    TestBed.runInInjectionContext(() => {
      filterAstManager = new FilterAstManager(null, subscription, undefined, undefined)
    });
    expect(filterAstManager).toBeTruthy();
  });
});
