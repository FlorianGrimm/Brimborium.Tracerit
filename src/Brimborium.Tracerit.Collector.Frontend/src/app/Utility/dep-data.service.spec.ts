import { TestBed } from '@angular/core/testing';

import { DepDataService } from './dep-data.service';
import { Subscription } from 'rxjs';
import { provideZonelessChangeDetection } from '@angular/core';

describe('DepDataService', () => {
  let service: DepDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
    service = TestBed.inject(DepDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('property should be created', () => {
    const subscription = new Subscription();

    const property = service.createProperty({
      name: 'test',
      initialValue: 1,
      subscription: subscription,
    });
    expect(property).toBeTruthy();
    expect(property.name).toEqual('test');
    expect(property.getValue()).toEqual(1);
  });

  it('property get/set', () => {
    const subscription = new Subscription();

    const property = service.createProperty({
      name: 'test',
      initialValue: 1,
      subscription: subscription,
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
      subscription: subscription,
    });
    //debugger;
    const propertyB = service.createProperty({
      name: 'testB',
      initialValue: 1,
      subscription: subscription
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner() },
      sourceTransform: (d) => { return d.propA + 1; }
    });

    expect(propertyA._getListSinkTrigger().length).toEqual(1);

    expect(propertyA).toBeTruthy();
    expect(propertyA.name).toEqual('testA');
    expect(propertyA.getValue()).toEqual(1);

    propertyA.setValue(2);
    expect(propertyA.getValue()).toEqual(2);
    expect(propertyB.getValue()).toEqual(3);

    for (let idx = 0; idx < 3; idx++) {
      propertyA.setValue(idx);
      expect(propertyB.getValue()).toEqual(idx + 1);
    }
    subscription.unsubscribe();
  });

  it('property dep 2 ui and inner', () => {
    const subscription = new Subscription();
    const propertyA = service.createProperty<number>({
      name: 'testA',
      initialValue: 1,
      subscription: subscription,
    });
    const propertyB = service.createProperty<number>({
      name: 'testB',
      initialValue: 1,
      subscription: subscription,
    });

    const listReport: string[] = [];
    const propertyRUi = service.createProperty({
      name: 'testRUi',
      initialValue: 1,
      report: (property, msg, value) => { listReport.push(`${property.name}-${value}`); },
      subscription: subscription
    }).withSource({
      sourceDependency: {
        propA: propertyA.dependencyUi(),
        propB: propertyB.dependencyUi()
      },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });

    const propertyRInner = service.createProperty({
      name: 'testRInner',
      initialValue: 1,
      report: (property, msg, value) => { listReport.push(`${property.name}-${value}`); },
      subscription: subscription,
    }).withSource({
      sourceDependency: {
        propA: propertyA.dependencyInner(),
        propB: propertyB.dependencyInner()
      },
      sourceTransform: (d) => { return d.propA + d.propB; }
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
      subscription: subscription,
    });
    const propertyB = service.createProperty<number>({
      name: 'testB',
      initialValue: 1,
      subscription: subscription,
    });

    const propertyRUi = service.createProperty({
      name: 'testRUi',
      initialValue: 1,
      subscription: subscription,
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyInner() },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });


    const propertyRInner = service.createProperty({
      name: 'testRInner',
      initialValue: 1,
      subscription: subscription,
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyInner() },
      sourceTransform: (d) => { return d.propA + d.propB; }
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

  it('property dep inner UI', () => {
    let index=1;
    const objectA = {};
    const depDataService = new DepDataService();
    const enhancedObjectA = depDataService.wrap(objectA);
    const propertyA = enhancedObjectA.createProperty<string>({
      name: 'testA',
      initialValue: "A",
    });
    const propertyB = enhancedObjectA.createProperty<string>({
      name: 'testB',
      initialValue: "B",
    });
    const propertyC = enhancedObjectA.createProperty<string>({
      name: 'testC',
      initialValue: "C",
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyUi() },
      sourceTransform: ({ propA, propB }) => { return `${index++}-${propA}-${propB}`;}
    });
    const propertyD = enhancedObjectA.createProperty<string>({
      name: 'testD',
      initialValue: "D",
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyPublic(), propC: propertyB.dependencyPublic() },
      sourceTransform: ({ propA, propC }) => { return `${index++}-${propA}-${propC}`;}
    });
    
    const propertyE = enhancedObjectA.createProperty<string>({
      name: 'testE',
      initialValue: "E",
    }).withSource({
      sourceDependency: { propB: propertyB.dependencyGate(), propC: propertyB.dependencyPublic() },
      sourceTransform: ({ propB, propC }, currentValue) => { 
        if (propB !== "BB") { return currentValue; }
        return `${index++}-${propB}-${propC}`;
      }
    });

    expect(propertyC.getValue()).toEqual('1-A-B');
    expect(propertyD.getValue()).toEqual('2-A-B');
    expect(propertyE.getValue()).toEqual('E');


    propertyA.setValue("AA");
    expect(propertyC.getValue()).toEqual('3-AA-B');
    expect(propertyD.getValue()).toEqual('4-AA-B');
    expect(propertyE.getValue()).toEqual('E');

    depDataService.start
    propertyB.setValue("BB");
    expect(propertyC.getValue()).toEqual('6-AA-BB');
    expect(propertyD.getValue()).toEqual('5-AA-BB');
    expect(propertyE.getValue()).toEqual('7-BB-BB');
  });

  it('property dep loop abort', () => {
    let index=1;
    const objectA = {};
    const depDataService = new DepDataService();
    const enhancedObjectA = depDataService.wrap(objectA);
    const propertyA = enhancedObjectA.createProperty<string>({
      name: 'testA',
      initialValue: "A",
    });
    const propertyB = enhancedObjectA.createProperty<string>({
      name: 'testB',
      initialValue: "B",
    });

    propertyA.withSource({
      sourceDependency: { propB: propertyB.dependencyPublic() },
      sourceTransform: ({ propB }) => { return `${index++}-A-${propB}`;}
    });

    propertyB.withSource({
      sourceDependency: { propA: propertyA.dependencyPublic() },
      sourceTransform: ({ propA }) => { return `${index++}-B-${propA}`;}
    });

    expect(index).toBeLessThan(10);    
  })
});
