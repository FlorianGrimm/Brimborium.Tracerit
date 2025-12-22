import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterAstNodeComponent } from './filter-ast-node.component';
import { provideZonelessChangeDetection, runInInjectionContext } from '@angular/core';
import { Subscription } from 'rxjs';
import { convertFilterAstNodeToUIFilterAstNode, UIFilterAstNode } from '@app/Utility/filter-ast-node';
import { FilterAstManager } from '../filter-ast-manager';
import { DepDataPropertyEnhancedObject, DepDataService } from '@app/Utility/dep-data.service';
import { DataService } from '@app/Utility/data-service';

describe('FilterAstNodeComponent', () => {
  let component: FilterAstNodeComponent;
  let fixture: ComponentFixture<FilterAstNodeComponent>;
  let filterAstManager: FilterAstManager;
  let depDataService: DepDataService;
  let depThis: DepDataPropertyEnhancedObject;
  let uiNodeRoot: UIFilterAstNode;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        DataService,
        DepDataService,
        provideZonelessChangeDetection()
      ],
      imports: [FilterAstNodeComponent]
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

    fixture = TestBed.createComponent(FilterAstNodeComponent);
    fixture.componentRef.setInput('filterAstManager', filterAstManager);
    fixture.componentRef.setInput('uiNode', uiNodeRoot);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
