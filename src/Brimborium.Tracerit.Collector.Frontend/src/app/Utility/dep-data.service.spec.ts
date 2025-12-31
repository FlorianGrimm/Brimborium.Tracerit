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

    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);
    depThis.executePropertyInitializer();
    expect(depThis).toBeTruthy();
  });

  it('property should be created', () => {
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);

    const property = depThis.createProperty({
      name: 'test',
      initialValue: 1,
    });
    depThis.executePropertyInitializer();

    expect(property).toBeTruthy();
    expect(property.name).toMatch(/Object\-\d+-test\-\d+/);
    expect(property.getValue()).toEqual(1);
  });

  it('property get/set', () => {
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);


    const property = depThis.createProperty({
      name: 'test',
      initialValue: 1,
    });
    expect(property).toBeTruthy();
    expect(property.name).toMatch(/Object\-\d+-test\-\d+/);
    expect(property.getValue()).toEqual(1);

    depThis.executePropertyInitializer();

    property.setValue(2);
    expect(property.getValue()).toEqual(2);
  });

  it('property dep', () => {
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);

    const propertyA = depThis.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });

    const propertyB = depThis.createProperty({
      name: 'testB',
      initialValue: 1,
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner() },
      sourceTransform: (d) => { return d.propA + 1; }
    });

    depThis.executePropertyInitializer();

    expect(propertyA._getListSinkTrigger().length).toEqual(1);

    expect(propertyA).toBeTruthy();
    expect(propertyA.name).toMatch(/Object\-\d+-testA\-\d+/);
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
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);

    const propertyA = depThis.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });
    const propertyB = depThis.createProperty<number>({
      name: 'testB',
      initialValue: 1,
    });

    const listReport: string[] = [];
    const propertyRUi = depThis.createProperty({
      name: 'testRUi',
      initialValue: 1,
      report: (property, msg, value) => { listReport.push(`${property.name}-${value}`); },
    }).withSource({
      sourceDependency: {
        propA: propertyA.dependencyUi(),
        propB: propertyB.dependencyUi()
      },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });

    const propertyRInner = depThis.createProperty({
      name: 'testRInner',
      initialValue: 1,
      report: (property, msg, value) => { listReport.push(`${property.name}-${value}`); },
    }).withSource({
      sourceDependency: {
        propA: propertyA.dependencyInner(),
        propB: propertyB.dependencyInner()
      },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });

    depThis.executePropertyInitializer();

    expect(propertyRUi.getValue()).toEqual(2);
    expect(propertyRInner.getValue()).toEqual(2);


    expect(listReport).toEqual([`${propertyRUi.name}-2`, `${propertyRInner.name}-2`]);

    propertyA.setValue(2);
    expect(propertyRUi.getValue()).toEqual(3);
    expect(propertyRInner.getValue()).toEqual(3);
    expect(listReport).toEqual([`${propertyRUi.name}-2`, `${propertyRInner.name}-2`, `${propertyRInner.name}-3`, `${propertyRUi.name}-3`]);

    subscription.unsubscribe();
  });

  it('property dep repeat', () => {

    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depThis = service.wrap(objectA);

    const propertyA = depThis.createProperty<number>({
      name: 'testA',
      initialValue: 1,
    });
    const propertyB = depThis.createProperty<number>({
      name: 'testB',
      initialValue: 1,
    });

    const propertyRUi = depThis.createProperty({
      name: 'testRUi',
      initialValue: 1,
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyInner() },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });


    const propertyRInner = depThis.createProperty({
      name: 'testRInner',
      initialValue: 1,
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyInner() },
      sourceTransform: (d) => { return d.propA + d.propB; }
    });

    depThis.executePropertyInitializer();

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
    let index = 1;
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depDataService = new DepDataService();
    const depThis = depDataService.wrap(objectA);

    const propertyA = depThis.createProperty<string>({
      name: 'testA',
      initialValue: "A",
    });
    const propertyB = depThis.createProperty<string>({
      name: 'testB',
      initialValue: "B",
    });
    const propertyC = depThis.createProperty<string>({
      name: 'testC',
      initialValue: "C",
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyInner(), propB: propertyB.dependencyUi() },
      sourceTransform: ({ propA, propB }) => { return `${index++}-${propA}-${propB}`; }
    });
    const propertyD = depThis.createProperty<string>({
      name: 'testD',
      initialValue: "D",
    }).withSource({
      sourceDependency: { propA: propertyA.dependencyPublic(), propC: propertyB.dependencyPublic() },
      sourceTransform: ({ propA, propC }) => { return `${index++}-${propA}-${propC}`; }
    });

    const propertyE = depThis.createProperty<string>({
      name: 'testE',
      initialValue: "E",
    }).withSource({
      sourceDependency: { propB: propertyB.dependencyGate(), propC: propertyB.dependencyPublic() },
      sourceTransform: ({ propB, propC }, currentValue) => {
        if (propB !== "BB") { return currentValue; }
        return `${index++}-${propB}-${propC}`;
      }
    });

    depThis.executePropertyInitializer();

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
    let index = 1;
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depDataService = new DepDataService();
    const depThis = depDataService.wrap(objectA);

    const propertyA = depThis.createProperty<string>({
      name: 'testA',
      initialValue: "A",
    });
    const propertyB = depThis.createProperty<string>({
      name: 'testB',
      initialValue: "B",
    });

    propertyA.withSource({
      sourceDependency: { propB: propertyB.dependencyPublic() },
      sourceTransform: ({ propB }) => { return `${index++}-A-${propB}`; }
    });

    propertyB.withSource({
      sourceDependency: { propA: propertyA.dependencyPublic() },
      sourceTransform: ({ propA }) => { return `${index++}-B-${propA}`; }
    });

    depThis.executePropertyInitializer();

    expect(index).toBeLessThan(10);
  });


  it('property dep prio', () => {
    let index = 1;
    const subscription = new Subscription();
    const objectA = { subscription: subscription };
    const depDataService = new DepDataService();
    const depThis = depDataService.wrap(objectA);

    const propertyA = depThis.createProperty<string>({
      name: 'testA',
      initialValue: "A",
      enableReport:true,
    });
    const propertyB = depThis.createProperty<string>({
      name: 'testB',
      initialValue: "B",
      enableReport:true,
    });
    
    const propertyE = depThis.createProperty<string>({
      name: 'testE',
      initialValue: "E",
      enableReport:true,
    });
    const propertyF = depThis.createProperty<string>({
      name: 'testF',
      initialValue: "F",
      enableReport:true,
    });

    const propertyC = depThis.createProperty<string>({
      name: 'testC',
      initialValue: "C",
      enableReport:true,
    });
    const propertyD = depThis.createProperty<string>({
      name: 'testD',
      initialValue: "D",
      enableReport:true,
    });

    propertyD.withSource({
      sourceDependency: { propC: propertyC.dependencyInner(), propB: propertyB.dependencyInner() },
      //sourceTransform: ({ propC, propB }) => { return `(${index++}-${propC}-${propB})`; }
      sourceTransform: ({ propC, propB }) => { return `D(${propC}-${propB})`; }
    });

    propertyC.withSource({
      sourceDependency: { propE: propertyE.dependencyInner(), propB: propertyB.dependencyInner() },
      //sourceTransform: ({ propA, propB }) => { return `(${index++}-${propA}-${propB})`; }
      sourceTransform: ({ propE, propB }) => { return `C(${propE}-${propB})`; }
    });

    propertyE.withSource({
      sourceDependency: { propA: propertyA.dependencyInner() },
      sourceTransform: ({ propA }) => `E(${propA})`
    });
    propertyF.withSource({
      sourceDependency: { propB: propertyB.dependencyInner() },
      sourceTransform: ({ propB }) => `F(${propB})`
    });

    depThis.executePropertyInitializer();

    index=100;
    const listReport:string[]=[] ;
    depDataService.report=(property, message, value)=>{
      listReport.push(`${property.objectPropertyIdentity.propertyName}-${value}`);
    };
    const scope = depDataService.start({});
    propertyB.setValue("b");
    propertyA.setValue("a");
    expect(propertyE.getIsDirty(), "propertyE").toBe(true);
    expect(propertyF.getIsDirty(), "propertyF").toBe(true);

    expect(propertyC.getIsDirty(), "propertyC").toBe(true);
    expect(propertyD.getIsDirty(), "propertyD").toBe(true);

    scope.executeTrigger();
    const actualD = propertyD.getValue();
    expect(actualD).toBe("D(C(E(a)-b)-b)");
    expect(listReport[0]).toBe("testB-b");
    expect(listReport[1]).toBe("testA-a");
    expect(listReport[2]).toBe("");
    expect(listReport[3]).toBe("");
    expect(listReport.length).toBe(4);
    
  });


});
