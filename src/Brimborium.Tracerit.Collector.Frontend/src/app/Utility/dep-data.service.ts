import { Injectable, Signal, signal, WritableSignal } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';

export type DepDataServiceEnd = (fnFinish: undefined | (() => void)) => void;

@Injectable({
  providedIn: 'root',
})
export class DepDataService {
  private _listTrigger: DepDataPropertyTrigger[] = [];
  private _PropertyIndex = 1;

  createProperty<V>(
    args: DepDataPropertyArguments<V>
  ): DepDataProperty<V> {
    return new DepDataProperty<V>(args, this, this._PropertyIndex++);
  }
  createPropertyDep<V, TS extends Record<string, DepDataDependency<any>>, KS extends keyof TS>(
    args: DepDataPropertyArgumentsSource<V, TS, KS>
  ): DepDataProperty<V> {
    return new DepDataProperty<V>(args, this, this._PropertyIndex++);
  }

  addTrigger(trigger: DepDataPropertyTrigger) {
    this._listTrigger.push(trigger);
  }

  start(triggerer: DepDataProperty<any>): DepDataServiceEnd {
    // if (this.currentTriggerIndex < 0) {
    //   return this._runTriggersNested ??= ((fnFinish: undefined | (() => void)) => { });
    // } else {
    // }
    return this._runTriggers ??= ((fnFinish: undefined | (() => void)) => {
      this.runTriggers(fnFinish);
    });
  }

  // caching handlers
  private _runTriggers: DepDataServiceEnd | undefined;
  //private _runTriggersNested: DepDataServiceEnd | undefined;

  currentTriggerIndex = -1;

  runTriggers(fnFinish: undefined | (() => void)) {
    if (fnFinish != null) {
      this._listTrigger.push({
        executeTrigger: fnFinish
      });
    }
    if (0 <= this.currentTriggerIndex) { return; }
    this.currentTriggerIndex = 0;
    try {
      for (; this.currentTriggerIndex < this._listTrigger.length; this.currentTriggerIndex++) {
        const trigger = this._listTrigger[this.currentTriggerIndex];
        try {
          trigger.executeTrigger();
        } catch (error) {
          console.error(error);
        }
      }
    } finally {
      this._listTrigger.length = 0;
      this.currentTriggerIndex = -1;
    }
  }
}

export class DepDataPropertyResults {
  private static _instance: DepDataPropertyResults | undefined;
  static instance() { return this._instance ??= new DepDataPropertyResults(); }

  public result<T>(value: T): DepDataPropertyResultSuccess<T> {
    return ({
      result: value,
      error: undefined
    });
  }

  public error(error: string): DepDataPropertyResultError {
    return ({
      result: undefined,
      error: error
    });
  }
}

export type DepDataPropertyTrigger = {
  executeTrigger(): void;
}

export type DepDataPropertyResult<V> = DepDataPropertyResultSuccess<V> | DepDataPropertyResultError;

export type DepDataPropertyResultSuccess<V> = {
  readonly result: V;
  readonly error: undefined;
};

export type DepDataPropertyResultError = {
  readonly result: undefined;
  readonly error: string;
};

export type DepDataPropertyFn<V> = () => DepDataPropertyResult<V>

export type DepDataPropertySourceDependencies<TS extends Record<string, DepDataDependency<any>>, KS extends keyof TS> = {
  [name in KS]: TS[name] extends DepDataDependency<infer T> ? DepDataDependency<T> : never;
}

export type DepDataPropertySourceValue<TS extends Record<string, DepDataDependency<any>>, KS extends keyof TS> = {
  [name in KS]: TS[name] extends DepDataDependency<infer T> ? T : never;
}

export type DepDataPropertySourceFN<V, TS extends Record<string, DepDataDependency<any>>, KS extends keyof TS> = (
  source: DepDataPropertySourceValue<TS, KS>,
  changes: KS[],
  results: DepDataPropertyResults
) => DepDataPropertyResult<V>;

export type DepDataPropertyArguments<V, TS extends Record<string, DepDataDependency<any>> = any, KS extends keyof TS = any>
  = DepDataPropertyArgumentsNoSource<V> | DepDataPropertyArgumentsSource<V, TS, KS>;
export type DepDataPropertyArgumentsNoSource<V> = {
  name: string;
  initialValue: V;
  transform?: (value: V) => V;
  report?: (name: string, value: V) => void;
  // subscription?: undefined;
  // sourceDependencies?: undefined;
  // source?: undefined;
};

export type DepDataPropertyArgumentsSource<V, TS extends Record<string, DepDataDependency<any>>, KS extends keyof TS> =
  DepDataPropertyArgumentsNoSource<V>
  & {
    subscription: Subscription;
    sourceDependencies: DepDataPropertySourceDependencies<TS, KS>;
    source: DepDataPropertySourceFN<V, TS, KS>;
  };

export class DepDataProperty<V, TS extends Record<string, DepDataDependency<any>> = any, KS extends keyof TS = any> {
  public readonly name: string;
  private value: V;
  private transform: ((value: V) => V) | undefined;
  private report: ((name: string, value: V) => void) | undefined;
  private subscription: Subscription | undefined;
  private listDepToProperty: DepDataProperty<V>[] = [];
  private listDepToDependenecy: DepDataDependency<V>[] = [];
  private valueSource: undefined | {
    sourceDependencies: DepDataPropertySourceDependencies<TS, KS>;
    source: DepDataPropertySourceFN<V, TS, KS>;
  } = undefined;
  private _asObserable: BehaviorSubject<V> | undefined = undefined;
  private _asSignal: WritableSignal<V> | undefined = undefined;
  private results = DepDataPropertyResults.instance();

  constructor(
    args: DepDataPropertyArguments<V, TS, KS>,
    private readonly depDataService: DepDataService,
    private readonly propertyIndex: number
  ) {
    this.name = args.name;
    this.transform = args.transform;
    this.report = args.report;

    const argsSource = ((args as DepDataPropertyArgumentsSource<V, TS, KS>)?.subscription != null) ? (args as DepDataPropertyArgumentsSource<V, TS, KS>) : undefined;
    if (argsSource != null) {
      this.subscription = argsSource.subscription;
      if (args.initialValue != null) {
        this.value = args.initialValue;
      } else {
        this.value = undefined as any;
      }
      this.valueSource = {
        sourceDependencies: argsSource.sourceDependencies,
        source: argsSource.source
      };
      for (const key in this.valueSource.sourceDependencies) {
        const dep = this.valueSource.sourceDependencies[key];
        dep.property.subscribeDependency(dep, this, this.subscription);
      }
      this.calculateValue();
    } else {
      this.subscription = undefined;
      if (args.initialValue != null) {
        this.value = args.initialValue;
      } else {
        throw new Error("initialValue is invalid");
      }
    }
  }

  private subscribeDependency(
    dependency: DepDataDependency<any>,
    target: DepDataProperty<any>,
    targetSubscription: Subscription
  ) {
    //const targetSubscription = target.subscription;
    if (targetSubscription == null) { throw new Error("subscription is null"); }

    if (dependency.kind === "inner") {
      //this.dependencies push after the last inner#
      const index = this.listDepToDependenecy.filter(item => item.kind === "inner").length;
      if (0 <= index && index < this.listDepToDependenecy.length) {
        this.listDepToDependenecy.splice(index, 0, dependency);
      } else {
        this.listDepToDependenecy.push(dependency);
      }
    } else if (dependency.kind === "public") {
      // listDependencyTo add after the last inner or public
      const index = this.listDepToDependenecy.filter(item => item.kind === "inner" || item.kind === "public").length;
      if (0 <= index && index < this.listDepToDependenecy.length) {
        this.listDepToDependenecy.splice(index, 0, dependency);
      } else {
        this.listDepToDependenecy.push(dependency);
      }
    } else if (dependency.kind === "ui") {
      this.listDepToDependenecy.push(dependency);
    } else {
      throw new Error("kind is invalid");
    }
    {
      dependency.setTargetProperty(target);
      const indexDep = this.listDepToProperty.indexOf(target);
      if (indexDep < 0) {
        this.listDepToProperty.push(target);
      }
    }
    targetSubscription.add(
      () => { this.unsubscribeDependency(dependency, target); }
    );
  }

  private unsubscribeDependency(
    dependency: DepDataDependency<any>,
    target: DepDataProperty<any>) {
    const indexDependenecy = this.listDepToDependenecy.indexOf(dependency);
    if (indexDependenecy < 0) { return; }
    this.listDepToDependenecy.splice(indexDependenecy, 1);

    const propertyIndex = this.propertyIndex;
    const needRemoved = (this.listDepToDependenecy.findIndex(item => propertyIndex === item.property.propertyIndex) < 0);
    if (needRemoved) {
      const indexDepToProperty = this.listDepToProperty.indexOf(target);
      if (indexDepToProperty < 0) { return; }
      this.listDepToProperty.splice(indexDepToProperty, 1);
    }
  }

  private isDirty = false;
  setDirty() {
    if (this.isDirty) {
      return false;
    } else {
      this.isDirty = true;
      return true;
    }
  }

  executeTrigger() {
    if (!this.isDirty) { return; }
    this.calculateValue();
    this.isDirty = false;
  }

  calculateValue() {
    if (this.valueSource == null) { return; }
    const sourceValues = {} as DepDataPropertySourceValue<TS, KS>;
    for (const key in this.valueSource.sourceDependencies) {
      const value = this.valueSource.sourceDependencies[key].property.getValue();
      sourceValues[key] = value as any;
    }
    const result = this.valueSource.source(sourceValues, [], this.results);
    if (result.error != null) {
      // TODO: handle error
      /*throw new Error(result.error);*/
    } else {
      this.setValue(result.result);
    }
  }

  setValue(value: V) {
    if (this.transform) {
      value = this.transform.call(this, value);
    }
    this.value = value;
    if (this.report) {
      this.report(this.name, value);
    }
    const end = this.depDataService.start(this);
    for (const dependency of this.listDepToDependenecy) {
      dependency.trigger();
    }
    end(this._postSetValue ?? (() => { this.postSetValue(); }));    
  }
  private _postSetValue: undefined | (() => void) = undefined;
  private postSetValue() {
    const asObserable = this._asObserable;
    if (asObserable != null) {    asObserable.next(this.value);   }
    const asSignal = this._asSignal;
    if (asSignal != null) {    asSignal.set(this.value);   }
  }
  getValue() {
    return this.value;
  }

  public dependencyInner() {
    const result = new DepDataDependency(this, "inner", this.depDataService);
    return result;
  }

  public dependencyPublic() {
    const result = new DepDataDependency(this, "public", this.depDataService);
    return result;
  }

  public dependencyUi() {
    const result = new DepDataDependency(this, "ui", this.depDataService);
    return result;
  }

  public asObserable() {
    const result = this._asObserable ??= new BehaviorSubject(this.value as V);
    return result;
  }

  public asSignal() {
    return this._asSignal ??= signal(
      this.value,
      {
        debugName: this.name
      }
    );
  }
}

export type DepDataDependencyKind = 'inner' | 'public' | 'ui';
export class DepDataDependency<TSource> {
  targetProperty: undefined | DepDataProperty<any, any, any>;

  constructor(
    public readonly property: DepDataProperty<TSource>,
    public readonly kind: DepDataDependencyKind,
    public readonly depDataService: DepDataService
  ) {
  }

  public trigger() {
    if (this.targetProperty != null) {
      if (this.targetProperty.setDirty()) {
        this.depDataService.addTrigger(this.targetProperty);
      }
    }
  }

  setTargetProperty(value: DepDataProperty<any, any, any>) {
    if (this.targetProperty != null) { throw new Error("target is already set"); }
    this.targetProperty = value;
  }
}