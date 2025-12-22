import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNameComponent } from './filter-ast-name.component';
import { provideZonelessChangeDetection, runInInjectionContext } from '@angular/core';
import { Subscription } from 'rxjs';
import { convertFilterAstNodeToUIFilterAstNode, UIFilterAstNode } from '@app/Utility/filter-ast-node';
import { FilterAstManager } from '../filter-ast-manager';
import { DepDataPropertyEnhancedObject, DepDataService } from '@app/Utility/dep-data.service';
import { DataService } from '@app/Utility/data-service';

describe('FilterAstNameComponent', () => {
  let component: FilterAstNameComponent;
  let fixture: ComponentFixture<FilterAstNameComponent>;
  let filterAstManager: FilterAstManager;
  let depDataService: DepDataService;
  let depThis: DepDataPropertyEnhancedObject;
  let uiNodeRoot: UIFilterAstNode;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FilterAstNameComponent
      ],
      providers: [
        DataService,
        DepDataService,
        provideZonelessChangeDetection()
      ]
    }).compileComponents();

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

    fixture = TestBed.createComponent(FilterAstNameComponent);
    fixture.componentRef.setInput('filterAstManager', filterAstManager);
    fixture.componentRef.setInput('uiNode', uiNodeRoot);
    component = fixture.componentInstance;

    await fixture.whenStable();
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
