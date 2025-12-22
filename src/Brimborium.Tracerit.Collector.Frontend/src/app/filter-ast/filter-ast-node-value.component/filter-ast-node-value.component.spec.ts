import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNodeValue } from './filter-ast-node-value.component';
import { provideZonelessChangeDetection } from '@angular/core';
import { Subscription } from 'rxjs';
import { convertFilterAstNodeToUIFilterAstNode, UIFilterAstNode } from '@app/Utility/filter-ast-node';
import { FilterAstManager } from '../filter-ast-manager';
import { DepDataPropertyEnhancedObject, DepDataService } from '@app/Utility/dep-data.service';
import { DataService } from '@app/Utility/data-service';

describe('FilterAstNodeValue', () => {
  let component: FilterAstNodeValue;
  let fixture: ComponentFixture<FilterAstNodeValue>;
  let filterAstManager: FilterAstManager;
  let depDataService: DepDataService;
  let depThis: DepDataPropertyEnhancedObject;
  let uiNodeRoot: UIFilterAstNode;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        DataService,
        DepDataService,
        provideZonelessChangeDetection()],
      imports: [FilterAstNodeValue]
    })
      .compileComponents();

    const objectA = { subscription: new Subscription() };
    depDataService = TestBed.inject(DepDataService);
    depThis = depDataService.wrap(objectA);
    uiNodeRoot = convertFilterAstNodeToUIFilterAstNode({
      operator: 'and',
      listChild: [
        {
          operator: 'eq',
          listChild: undefined,
          value: {
            name: 'abc',
            typeValue: 'str',
            value: 'def',
          },
        }
      ],
      value: undefined,
    });

    TestBed.runInInjectionContext(() => {
      filterAstManager = new FilterAstManager(uiNodeRoot, objectA.subscription, undefined, undefined);
    });

    fixture = TestBed.createComponent(FilterAstNodeValue);
    fixture.componentRef.setInput('filterAstManager', filterAstManager);
    fixture.componentRef.setInput('uiNode', uiNodeRoot);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
