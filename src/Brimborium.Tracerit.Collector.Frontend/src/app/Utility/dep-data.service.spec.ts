import { TestBed } from '@angular/core/testing';

import { DepDataDependency, DepDataService } from './dep-data.service';
import { Subscription } from 'rxjs';

describe('DepDataService', () => {
  let service: DepDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DepDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('property should be created', () => {
    const property = service.createProperty({
      name: 'test',
      initialValue: 1,
    });
    expect(property).toBeTruthy();
    expect(property.name).toEqual('test');
    expect(property.getValue()).toEqual(1);
  });
  
  it('property get/set', () => {
    const property = service.createProperty({
      name: 'test',
      initialValue: 1,
    });
    expect(property).toBeTruthy();
    expect(property.name).toEqual('test');
    expect(property.getValue()).toEqual(1);

    property.setValue(2);
    expect(property.getValue()).toEqual(2);
  });

  

  it('property dep', () => {
    const subscription = new Subscription();
    const propertyA = service.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });
    //debugger;
    const propertyB = service.createPropertyDep({
      name: 'testB',
      initialValue: 1,
      subscription: subscription,
      sourceDependencies: { propA: propertyA.dependencyInner() },
      source: (d,chg) => { return { result: d.propA+1, error: undefined }; }
    });
    expect(propertyA).toBeTruthy();
    expect(propertyA.name).toEqual('testA');
    expect(propertyA.getValue()).toEqual(1);

    propertyA.setValue(2);
    expect(propertyA.getValue()).toEqual(2);
    subscription.unsubscribe();
  });

    it('property dep', () => {
    const subscription = new Subscription();
    const propertyA = service.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });
    const propertyB = service.createProperty<number>({
      name: 'testB',
      initialValue: 1,
    });

    const listReport: string[] = [];
    const propertyRUi = service.createPropertyDep({
      name: 'testRUi',
      initialValue: 1,
      report: (name, value) => {  listReport.push(`${name}-${value}`); },
      subscription: subscription,
      sourceDependencies: { propA: propertyA.dependencyUi(),propB: propertyB.dependencyUi() },
      source: (d,chg) => { return { result: d.propA+d.propB, error: undefined }; }
    });
    
    
    const propertyRInner = service.createPropertyDep({
      name: 'testRInner',
      initialValue: 1,
      report: (name, value) => {  listReport.push(`${name}-${value}`); },
      subscription: subscription,
      sourceDependencies: { propA: propertyA.dependencyInner(),propB: propertyB.dependencyInner() },
      source: (d,chg) => { return { result: d.propA+d.propB, error: undefined }; }
    });

    expect(propertyRUi.getValue()).toEqual(2);
    expect(propertyRInner.getValue()).toEqual(2);

    expect(listReport).toEqual(['testRUi-2', 'testRInner-2']);

    propertyA.setValue(2);
    expect(propertyRUi.getValue()).toEqual(3);
    expect(propertyRInner.getValue()).toEqual(3);
    expect(listReport).toEqual(['testRUi-2', 'testRInner-2', 'testRInner-3', 'testRUi-3']);

    subscription.unsubscribe();
  });

  
    it('property dep repeat', () => {
    const subscription = new Subscription();
    const propertyA = service.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });
    const propertyB = service.createProperty<number>({
      name: 'testB',
      initialValue: 1,
    });

    const propertyRUi = service.createPropertyDep({
      name: 'testRUi',
      initialValue: 1,
      subscription: subscription,
      sourceDependencies: { propA: propertyA.dependencyUi(),propB: propertyB.dependencyUi() },
      source: (d,chg) => { return { result: d.propA+d.propB, error: undefined }; }
    });
    
    
    const propertyRInner = service.createPropertyDep({
      name: 'testRInner',
      initialValue: 1,
      subscription: subscription,
      sourceDependencies: { propA: propertyA.dependencyInner(),propB: propertyB.dependencyInner() },
      source: (d,chg) => { return { result: d.propA+d.propB, error: undefined }; }
    });

    expect(propertyRUi.getValue()).toEqual(2);
    expect(propertyRInner.getValue()).toEqual(2);

    for (let idx = 0; idx < 100; idx++) {
      propertyA.setValue(idx);
      expect(propertyRUi.getValue()).toEqual(idx + 1);
      expect(propertyRInner.getValue()).toEqual(idx + 1);
    }

    subscription.unsubscribe();
  });
});
