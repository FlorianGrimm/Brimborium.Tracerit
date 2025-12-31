import {
  effect, Injectable, Signal, signal, untracked, WritableSignal,
  OnDestroy, OnInit,
  EffectRef,
} from '@angular/core';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { BehaviorSubject, InteropObservable, Subscribable, Subscription } from 'rxjs';

export type DepDataServiceEnd = (fnFinish: undefined | (() => void)) => void;

export type TransformFN<V> = (value: V) => V;
export type ReportFN<V> = ((property: DepDataProperty<V>, message: string, value: V) => void);
export type ReportErrorFN = ((that: any, classMethod: string, error: unknown) => void);

export type DepDataPropertySourceDependency<TS> = {
  [name in keyof TS]: IDepDataPropertyDependency<TS[name]>;
}
export type DepDataPropertySourceValue<TS> = {
  [name in keyof TS]: TS[name];
}

export type DepDataPropertyForSignalArgumentsE<V> = {
  /* name is used for debugging. */
  name: string;

  /* compare is used to compare the new value with the old value - if compare returns true the new value is not set. */
  compare?: ((a: V, b: V) => boolean) | undefined;

  /* sideEffect is called after the value is set. */
  sideEffect?: DepDataSideEffectTriggerArguments<V>;

  enableReport?: boolean;

  /* report is called after the value is set - before the dependcies and sideEffect is called. */
  report?: ReportFN<V> | undefined;
};

export type DepDataPropertyValueArgumentsE<V> = {
  /* name is used for debugging. */
  name: string;

  /* initialValue is used for the initial value. */
  initialValue: V;

  /* input connects this property to angular input(). */
  input?: undefined | DepDataPropertyInputArguments<V> | DepDataPropertyInputAndTransformArguments<any, V>;
  /* output TODO */

  /* transform is used to transform the new value before it is set. */
  transform?: TransformFN<V> | undefined;

  /* compare is used to compare the new value with the old value - if compare returns true the new value is not set. */
  compare?: ((a: V, b: V) => boolean) | undefined;

  /* sideEffect is called after the value is set. */
  sideEffect?: DepDataSideEffectTriggerArguments<V>;

  enableReport?: boolean;

  /* report is called after the value is set - before the dependcies and sideEffect is called. */
  report?: ReportFN<V> | undefined;
};

export type DepDataPropertyEnhancedObjectArguments = {
  objectName: string;
  objectIndex: number;
  propertyName: string;
  propertyIndex: number;
  /* subscription is used to unsubscribe all dependcies and sideEffect. */
  subscription: Subscription;
};

export type DepDataPropertyValueArgumentsComplete<V> =
  DepDataPropertyValueArgumentsE<V>
  & DepDataPropertyEnhancedObjectArguments;

export type DepDataPropertyForSignalArgumentsComplete<V> =
  DepDataPropertyForSignalArgumentsE<V>
  & DepDataPropertyEnhancedObjectArguments;

export type DepDataPropertyInputArguments<V> = {
  input: Signal<V>;
};

export type DepDataPropertyInputAndTransformArguments<S, V> = {
  input: Signal<S>;
  transform: (value: S, currentValue: V, logicalTime: number) => V;
};

export interface DepDataPropertyTrigger {
  name?: string | undefined;
  setDirty(): boolean;
  executeTrigger(scope: DepDataServiceExecutionScope): void;
  getDirtyDeps(): number;
};

export type DepDataPropertyTriggerKind =
  /* Inner is called immediately. */
  "Inner"
  |
  /* Public is called after the Inner ones. */
  "Public"
  |
  /* UI is called after the Public ones. */
  "UI";

export type DepDataPropertyTriggerAndKind = {
  trigger: DepDataPropertyTrigger,
  kind: DepDataPropertyTriggerKind
};

export type DepDataServiceStart = {
  name?: string;
  pauseNestedExecution?: boolean;
  delayedExecution?: boolean;
};

export type DepDataPropertyEnhancedThat = Partial<OnDestroy>
  & Partial<OnInit>
  & Partial<{
    depDataPropertyInitializer: DepDataPropertyInitializer;
    subscription: Subscription;
  }>;

export type LogEntry = {
  objectPropertyIdentity: ObjectPropertyIdentity;
  logicalTime: number;
  watchDog: number;
  message: string;
  value: any;
};

export type ObjectPropertyIdentity = {
  readonly objectName: string;
  readonly objectIndex: number;
  readonly propertyName: string;
  readonly propertyIndex: number;
};

@Injectable({
  providedIn: 'root',
})
export class DepDataService {
  private _PropertyIndex = 1;
  constructor() { }

  public wrap(
    that: DepDataPropertyEnhancedThat
  ) {
    return new DepDataPropertyEnhancedObject(that, this, () => this.nextPropertyIndex());
  }

  private nextPropertyIndex() {
    return this._PropertyIndex++;
  }

  private readonly _ExecutionScope = new DepDataServiceExecutionManager(this);

  /** starts a new transaction. */
  public start(args: DepDataServiceStart) {
    return this._ExecutionScope.create(args);
  }
  public scoped(action: () => void, args?: DepDataServiceStart) {
    const transaction = this._ExecutionScope.create(args);
    try {
      action();
    } finally {
      transaction.executeTrigger(transaction);
    }
  }

  public addTriggerToQueue(trigger: DepDataPropertyTrigger, kind?: DepDataPropertyTriggerKind) {
    const scope = this._ExecutionScope.getScope();
    scope.addTriggerToQueue(trigger, kind ?? "UI");
  }

  /** custom callback for onReport. */
  public report: ReportFN<any> | undefined;

  /** calls the custom callback for onReport or the default one. */
  public onReport<V>(property: DepDataProperty<V>, message: string, value: V) {
    if (this.report) {
      this.report(property, message, value);
    } else {
      if (value === undefined) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, undefined);
      } else if (value === null) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, null);
      } else if (Array.isArray(value)) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, value.length);
      } else if (value instanceof ZonedDateTime) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, value.toString());
      } else if (value instanceof Duration) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, value.toString());
      } else if (value instanceof Date) {
        console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, value.toISOString());
      } else {
        if (typeof value === 'object') {
          const listKey = Object.keys(value);
          if (listKey.length < 10) {
            const valueToString = {} as Record<string, string>;
            for (const key of listKey) {
              try {
                const item: any = (value as any)[key];
                if (item == null) { continue; }
                if (Array.isArray(item)) {
                  valueToString[key] = `Array(${item.length})`;
                } else if (item instanceof Date) {
                  valueToString[key] = item.toISOString();
                } else if ("function" === typeof item.toString) {
                  const itemToString = item.toString();
                  if (40 < itemToString.length) {
                    valueToString[key] = itemToString.substring(0, 40);
                  } else {
                    valueToString[key] = itemToString;
                  }
                } else {
                  valueToString[key] = item;
                }
              } catch {
              }
            }
            console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, valueToString);
            return;
          }
        }
        {
          console.log(`Property ${property.name}-${property.objectPropertyIdentity.propertyIndex}-${property.logicalTime} ${message}`, value);
        }
      }
    }
  }

  /** custom callback for onReportError. */
  public reportError: ReportErrorFN | undefined;

  /** calls the custom callback for onReportError or the default one. */
  public onReportError(that: any, classMethod: string, error: unknown) {
    if (this.reportError) {
      this.reportError(that, classMethod, error);
    } else {
      console.error(classMethod, error);
    }
  }


  private _isLoggingEnabled: boolean = false;
  public get isLoggingEnabled(): boolean {
    return this._isLoggingEnabled;
  }
  public set isLoggingEnabled(value: boolean) {
    this._isLoggingEnabled = value;
  }

  ListLogEntry: LogEntry[] = [];
  addLog(logEntry: LogEntry) {
    if (!this._isLoggingEnabled) { return; }
    if (1000 < this.ListLogEntry.length) {
      this.ListLogEntry.splice(0, 500);
    }
    this.ListLogEntry.push(logEntry);
    // console.log({ ...logEntry.objectPropertyIdentity, ...logEntry });
  }

}

export class DepDataPropertyEnhancedObject {
  private static _nextObjectIndex = 1;
  public readonly objectIndex: number = DepDataPropertyEnhancedObject._nextObjectIndex++;

  public readonly objectName: string;
  public readonly depDataService: DepDataService;
  public readonly depDataPropertyInitializer: DepDataPropertyInitializer;
  public readonly subscription: Subscription;

  private nextPropertyIndex: () => number;

  constructor(
    that: DepDataPropertyEnhancedThat,
    depDataService: DepDataService,
    nextPropertyIndex: () => number
  ) {
    this.objectName = that.constructor.name;
    this.depDataService = depDataService;
    this.depDataPropertyInitializer = that.depDataPropertyInitializer ?? new DepDataPropertyInitializer(that, this.depDataService);
    this.subscription = that.subscription ?? new Subscription();
    if (that.ngOnInit == null) {
      that.ngOnInit = () => {
        this.depDataPropertyInitializer.execute(this.depDataService);
      };
    }
    if (that.ngOnDestroy == null) {
      that.ngOnDestroy = () => {
        this.unsubscribe();
      };
    }
    this.nextPropertyIndex = nextPropertyIndex;
  }

  public unsubscribe() {
    this.subscription.unsubscribe();
  }


  /** creates a new property. */
  public createProperty<V>(
    args: DepDataPropertyValueArgumentsE<V>
  ): DepDataPropertyValue<V> {
    const propertyIndex = this.nextPropertyIndex()
    const argsComplete: DepDataPropertyValueArgumentsComplete<V> = {
      name: `${this.objectName}-${this.objectIndex}-${args.name}-${propertyIndex}`,
      objectName: this.objectName,
      objectIndex: this.objectIndex,
      propertyName: args.name,
      propertyIndex: propertyIndex,
      initialValue: args.initialValue,
      input: args.input,
      transform: args.transform,
      compare: args.compare,
      sideEffect: args.sideEffect,
      enableReport: args.enableReport,
      report: args.report,
      subscription: this.subscription
    };
    return new DepDataPropertyValue<V>(argsComplete, this.depDataService, this.nextPropertyIndex(), this.depDataPropertyInitializer);
  }

  public createPropertyForSignal<V>(
    signal: WritableSignal<V>,
    args: DepDataPropertyForSignalArgumentsE<V>
  ): DepDataPropertyForSignal<V> {
    const propertyIndex = this.nextPropertyIndex()
    const argsComplete: DepDataPropertyForSignalArgumentsComplete<V> = {
      name: `${this.objectName}-${this.objectIndex}-${args.name}-${propertyIndex}`,
      objectName: this.objectName,
      objectIndex: this.objectIndex,
      propertyName: args.name,
      propertyIndex: propertyIndex,
      compare: args.compare,
      sideEffect: args.sideEffect,
      enableReport: args.enableReport,
      report: args.report,
      subscription: this.subscription
    };
    return new DepDataPropertyForSignal<V>(
      signal,
      argsComplete, this.depDataService, this.nextPropertyIndex(), this.depDataPropertyInitializer);
  }



  public dependencyInput<V>(input: Signal<V>): IDepDataPropertyDependency<V> {
    return this.createProperty({
      name: 'dependencyInput',
      initialValue: input(),
      input: { input },
    }).dependencyInner();
  }

  public executePropertyInitializer() {
    this.depDataPropertyInitializer.execute(this.depDataService);
  }
}

/** allows to delay the creation of a property *source*. */
export class DepDataPropertyInitializer {
  private static _EnsureExecuted: DepDataPropertyInitializer[] | undefined = undefined;
  private static ensureExecuted(service: DepDataService) {
    if (DepDataPropertyInitializer._EnsureExecuted == null) { return; }
    const list = DepDataPropertyInitializer._EnsureExecuted;
    DepDataPropertyInitializer._EnsureExecuted = undefined;
    for (const initializer of list) {
      if (initializer._ListDelayed == null) { continue; }
      console.warn(`Missing call to DepDataPropertyInitializer.execute for ${initializer.that?.constructor.name}`);
      initializer.execute(service);
    }
  }

  private _ListDelayed: DepDataPropertyWithSourceDelayed<any, any>[] | undefined = undefined;

  that: {} | undefined;;
  constructor(
    that: {},
    service: DepDataService
  ) {
    this.that = that;
    if (DepDataPropertyInitializer._EnsureExecuted == null) {
      window.requestAnimationFrame(() => { DepDataPropertyInitializer.ensureExecuted(service); });
      DepDataPropertyInitializer._EnsureExecuted = [];
    }
    DepDataPropertyInitializer._EnsureExecuted.push(this);
  }

  /** internal - add a delayed creation. */
  public add<TS>(delayed: DepDataPropertyWithSourceDelayed<any, any>) {
    const list = (this._ListDelayed ??= []);
    list.push(delayed);
  }

  /** internal - execute all delayed creations. */
  public execute(service: DepDataService) {
    const listDelayed = this._ListDelayed;
    this._ListDelayed = undefined;
    if (listDelayed == null) { return; }
    if (this.that == null) { return; }
    const scope = service.start({
      name: 'DepDataPropertyInitializer.execute'
    });
    try {
      for (const delayed of listDelayed) {
        delayed.fnDelayed(delayed, scope);
      }
    } finally {
      scope.executeTrigger(scope);
    }
    this.that = undefined;;
  }
}

export type SourceDependency<TS> = DepDataPropertySourceDependency<TS>;
export type SourceTransform<TS, V> = (value: DepDataPropertySourceValue<TS>, currentValue: V, scope: DepDataServiceExecutionScope) => V;

/** arguments for withSource. */
export type DepDataPropertyWithSource<TS, V> = {
  //sourceDependency: DepDataPropertySourceDependency<TS>;
  sourceDependency: SourceDependency<TS>;
  //sourceTransform: (value: DepDataPropertySourceValue<TS>, currentValue: V) => V;
  sourceTransform: SourceTransform<TS, V>;
  subscription?: Subscription;
  depDataPropertyInitializer?: DepDataPropertyInitializer;
}

/** arguments for withSource with delayed execution. */
export type DepDataPropertyWithSourceDelayed<TS, V> =
  DepDataPropertyWithSource<TS, V>
  & {
    fnDelayed: (args: DepDataPropertyWithSourceDelayed<TS, V>, scope: DepDataServiceExecutionScope) => void;
  }

export interface DepDataProperty<V> extends InteropObservable<V> {
  readonly name: string;
  readonly objectPropertyIdentity: ObjectPropertyIdentity;
  //readonly propertyIndex: number;
  logicalTime: number;
  weight: number;

  getValue(): V;
  setValue(value: V, scope?: DepDataServiceExecutionScope): void;

  // dependencyInner(): IDepDataPropertyDependency<V>;
  // dependencyPublic(): IDepDataPropertyDependency<V>;
  // dependencyUi(): IDepDataPropertyDependency<V>;
  // dependencyGate(): IDepDataPropertyDependency<V>;

  // withSource<TS>(
  //   args: DepDataPropertyWithSource<TS, V>
  // ): this;
  // withSourceIdentity(
  //   dependency: IDepDataPropertyDependency<V>
  // ): this;

  addSinkTrigger(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind): void;
  removeSinkTrigger(trigger: DepDataPropertyTrigger): void;
  getDirtyDeps(): number;
  getIsDirty(): boolean;
  // asSignal(): Signal<V>;
  // asObserable(): BehaviorSubject<V>;

  // _getListSinkTrigger(): { trigger: DepDataPropertyTrigger, kind: string }[]
}

export class DepDataPropertyBase<V> implements DepDataProperty<V>, InteropObservable<V> {
  public readonly name: string;
  public readonly subscription: Subscription;
  public readonly objectPropertyIdentity: ObjectPropertyIdentity;

  public _listSource: DepDataServiceSource<V, any>[] | undefined = undefined;
  public _listSinkTrigger: ListDepDataPropertyTriggerAndKind = new ListDepDataPropertyTriggerAndKind();

  public logicalTime: number = 0;
  public weight: number = 0;

  constructor(
    protected _service: DepDataService,
    protected depDataPropertyInitializer: DepDataPropertyInitializer,
    name: string,
    subscription: Subscription,
    objectPropertyIdentity: ObjectPropertyIdentity

  ) {
    this.name = name;
    this.subscription = subscription;
    this.objectPropertyIdentity = objectPropertyIdentity;
  }

  public getValue(): V {
    throw new Error("getValue is abstact");
  }
  public setValue(value: V, scope?: DepDataServiceExecutionScope): void {
    throw new Error("setValue is abstact");
  }

  getDirtyDeps(): number {
    if (this._listSource == null) {
      return 0;
    } else {
      let result = 0;
      for (let index = 0; index < this._listSource.length; index++) {
        const source = this._listSource[index];
        result += source.getDirtyDeps();
      }
      return result;
    }
  }

  getIsDirty(): boolean {
    if (this._listSource != null) {
      for (const source of this._listSource) {
        if (source.getIsDirty()) {
          return true;
        }
      }
    }
    return false;
  }

  public withSource<TS>(
    args: DepDataPropertyWithSource<TS, V>
  ) {
    if (args.depDataPropertyInitializer == null) {
      args.depDataPropertyInitializer = this.depDataPropertyInitializer;
    }
    if (args.depDataPropertyInitializer == null) {
      const scope = this._service.start({
        name: this.name
      });
      try {
        this._internalWithSource(args, scope);
      } finally {
        scope.executeTrigger(scope);
      }
    } else {
      args.depDataPropertyInitializer.add({
        ...args,
        fnDelayed: this._internalWithSource.bind(this),
      });
    }
    return this;
  }

  /** use this as a dependency. */
  public dependencyInner(): IDepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("Inner", this); };

  /** use this as a dependency. */
  public dependencyPublic(): IDepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("Public", this); };

  /** use this as a dependency. */
  public dependencyUi(): IDepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("UI", this); };

  public dependencyGate(): IDepDataPropertyDependency<V> { return new DepDataPropertyDependencyGate<V>(this); };

  public withSourceIdentity(
    dependency: IDepDataPropertyDependency<V>
  ) {
    return this.withSource<{ value: V }>({
      sourceDependency: { value: dependency },
      sourceTransform: (d) => d.value,
      depDataPropertyInitializer: this.depDataPropertyInitializer,
    });
  }

  private _internalWithSource<TS>(
    args: DepDataPropertyWithSource<TS, V>,
    scope: DepDataServiceExecutionScope
  ) {
    let subscription = args.subscription;
    if (subscription == null) {
      subscription = this.subscription;
    } else {
      this.subscription.add(subscription);
    }
    const source = new DepDataServiceSource<V, TS>(args.sourceDependency, args.sourceTransform, this, this._service);
    subscription.add(source);

    const listSource = (this._listSource ??= []);
    listSource.push(source);
    const sourceWeight = source.getWeight()
    if (this.weight < sourceWeight) {
      this.weight = sourceWeight;
    }
    this._listSource.sort((a, b) => a.weight - b.weight);
    source.updateValue(scope);
    return this;
  }

  unsubscribe(): void {
    const listSource = this._listSource;
    this._listSource = undefined;
    if (listSource != null) {
      for (const source of listSource) {
        source.unsubscribe();
      }
    }
    this._listSinkTrigger.clear();
    this._isClosed = true;
  }
  private _isClosed: boolean = false;
  get closed(): boolean { return this._isClosed; }

  addSinkTrigger(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    this._listSinkTrigger.add(trigger, kind);
  }

  removeSinkTrigger(trigger: DepDataPropertyTrigger) {
    this._listSinkTrigger.remove(trigger);
  }

  [Symbol.observable](): Subscribable<V> {
    return this.asObserable();
  }

  protected subject: DepDataServiceSubject<V> | undefined;
  public asObserable(): BehaviorSubject<V> {
    const subject = this.subject ??= new DepDataServiceSubject<V>(this);
    return subject;
  }

  _getListSinkTrigger(): { trigger: DepDataPropertyTrigger, kind: string }[] {
    return Array.from(this._listSinkTrigger);
  }
}

/** property - with a value if set the 
 * value can be transformed
 * and can be compared with the old value (and skipped if equal)
 * and the value is set.
 * Than the dependencies are recalculated and the sideEffects are called. 
 */
export class DepDataPropertyValue<V> extends DepDataPropertyBase<V> implements DepDataProperty<V>, InteropObservable<V> {
  public value: V;
  private _transform: ((value: V) => V) | undefined;
  private _compare: ((a: V, b: V) => boolean) | undefined;
  private _enableReport: boolean;
  private _report: ReportFN<V> | undefined;
  private sideEffect: DepDataSideEffectTriggerArguments<V> | undefined;
  private sideEffectTrigger: DepDataSideEffectTrigger<V> | undefined;

  constructor(
    args: DepDataPropertyValueArgumentsComplete<V>,
    service: DepDataService,
    public readonly propertyIndex: number,
    depDataPropertyInitializer: DepDataPropertyInitializer
  ) {
    super(
      service,
      depDataPropertyInitializer,
      args.name,
      args.subscription,
      {
        objectName: args.objectName,
        objectIndex: args.objectIndex,
        propertyName: args.propertyName,
        propertyIndex: args.propertyIndex,
      });

    this._transform = args.transform;
    this._compare = args.compare;
    this._enableReport = args.enableReport ?? false;
    this._report = args.report;
    this.value = args.initialValue;
    if (args.sideEffect == null) {
      this.sideEffect = undefined;
      this.sideEffectTrigger = undefined;
    } else {
      this.sideEffect = args.sideEffect;
      const kind = args.sideEffect.kind ?? "UI";
      this.sideEffectTrigger = new DepDataSideEffectTrigger<V>(this, this.sideEffect, kind);
    }
    this._listSinkTrigger.name = this.name;

    // input as a source for prop
    if (args.input != null) {
      const argsInput = args.input as DepDataPropertyInputArguments<V> | DepDataPropertyInputAndTransformArguments<any, V>;
      const inputSignal = argsInput.input;
      if ("function" === typeof (argsInput as DepDataPropertyInputAndTransformArguments<any, V>).transform) {
        const transform = (argsInput as DepDataPropertyInputAndTransformArguments<any, V>).transform;
        const watcher = effect(() => {
          const valueS = inputSignal();
          untracked(() => {
            const valueV = transform(valueS, this.getValue(), this.logicalTime);
            this.setValue(valueV);
          });
        });
        this.subscription.add(() => { watcher.destroy(); });
      } else {
        const watcher = effect(() => {
          const valueV = inputSignal();
          untracked(() => {
            this.setValue(valueV);
          });
        });
        this.subscription.add(() => { watcher.destroy(); });
      }
    }
  }

  public override getValue(): V {
    return this.value;
  }

  private _WatchDog: number = 0;

  public override setValue(value: V, scope?: DepDataServiceExecutionScope) {
    if (this.closed) {
      this._service.onReportError(this, 'DepDataProperty.setValue', new Error("closed"));
      return;
    }
    if (this._transform != null) {
      value = this._transform(value);
    }
    if (this._compare != null) {
      if (this._compare(this.value, value)) {
        return;
      }
    }
    const transaction = scope ?? this._service.start({
      name: this.name
    });

    this.value = value;
    if (this.logicalTime == transaction.id) {
      if (10 < (this._WatchDog++)) {
        debugger;
        throw new Error("WatchDog");
      }
    } else {
      this._WatchDog = 0;
      this.logicalTime = transaction.id;
    }

    const logEntry: LogEntry = {
      objectPropertyIdentity: this.objectPropertyIdentity,
      logicalTime: this.logicalTime,
      watchDog: this._WatchDog,
      message: 'setValue',
      value: value,
    };
    this._service.addLog(logEntry);
    if (this._report != null) {
      this._report(this, 'set', value);
    }
    if (this._enableReport) {
      this._service.onReport(this, 'set', value);
    }

    try {
      for (const trigger of this._listSinkTrigger) {
        //transaction.addTriggerOrExecute(trigger.trigger, trigger.kind);
        transaction.addTriggerToQueue(trigger.trigger, trigger.kind);
      }
      if (this.signal != null) {
        transaction.addTriggerToQueue(this.signal, "UI");
      }
      if (this.subject != null) {
        transaction.addTriggerToQueue(this.subject, "UI");
      }
      if (this.sideEffect != null && this.sideEffectTrigger != null) {
        //transaction.addTriggerOrExecute(this.sideEffectTrigger, this.sideEffectTrigger.kind);
        transaction.addTriggerToQueue(this.sideEffectTrigger, this.sideEffectTrigger.kind);
      }
    } finally {
      if (scope == null) {
        transaction.executeTrigger(transaction);
      }
    }
  }

  private signal: DepDataServiceReadonlySignal<V> | undefined;
  public asSignal(): Signal<V> {
    const signal = this.signal ??= new DepDataServiceReadonlySignal<V>(this);
    return signal.readonlySignal;
  }
}

export class DepDataPropertyForSignal<V> extends DepDataPropertyBase<V> implements DepDataProperty<V>, InteropObservable<V> {
  public value: V;
  private _compare: ((a: V, b: V) => boolean) | undefined;
  private _enableReport: boolean;
  private _report: ReportFN<V> | undefined;
  private sideEffect: DepDataSideEffectTriggerArguments<V> | undefined;
  private sideEffectTrigger: DepDataSideEffectTrigger<V> | undefined;
  private signalSetScope: DepDataServiceExecutionScope | undefined;

  constructor(
    public readonly signal: WritableSignal<V>,
    args: DepDataPropertyForSignalArgumentsComplete<V>,
    _service: DepDataService,
    public readonly propertyIndex: number,
    depDataPropertyInitializer: DepDataPropertyInitializer
  ) {
    super(
      _service,
      depDataPropertyInitializer,
      args.name,
      args.subscription,
      {
        objectName: args.objectName,
        objectIndex: args.objectIndex,
        propertyName: args.propertyName,
        propertyIndex: args.propertyIndex,
      });


    this._compare = args.compare;
    this._enableReport = args.enableReport ?? false;
    this._report = args.report;
    if (args.sideEffect == null) {
      this.sideEffect = undefined;
      this.sideEffectTrigger = undefined;
    } else {
      this.sideEffect = args.sideEffect;
      const kind = args.sideEffect.kind ?? "UI";
      this.sideEffectTrigger = new DepDataSideEffectTrigger<V>(this, this.sideEffect, kind);
    }
    this._listSinkTrigger.name = this.name;

    this.signal = signal;

    {
      let initialValue: V = undefined! as any;
      // initial value
      untracked(() => { initialValue = signal(); });
      this.value = initialValue;
    }

    // handle changed value
    {
      const effectRef = effect(() => {
        const valueS = signal();
        console.log("signal value ", valueS);
        const scope = this.signalSetScope;
        this.signalSetScope = undefined;

        this.setValueFromSignal(valueS, scope);
      })
      this.subscription.add(() => {
        effectRef.destroy();
      });
    }
  }

  public override getValue(): V {
    return this.value;
  }

  private _WatchDog: number = 0;

  public override  setValue(value: V, scope?: DepDataServiceExecutionScope) {
    // set the signal, effect will call setValueFromSignal
    this.signalSetScope = scope;
    this.signal.set(value);
  }

  private setValueFromSignal(value: V, scope?: DepDataServiceExecutionScope) {
    const oldValue = this.value;
    this.value = value;
    if (this.closed) {
      this._service.onReportError(this, 'DepDataProperty.setValue', new Error("closed"));
      return;
    }
    if (this._compare != null) {
      if (this._compare(oldValue, value)) {
        return;
      }
    }
    const transaction = scope ?? this._service.start({
      name: this.name
    });
    if (this.logicalTime == transaction.id) {
      if (10 < (this._WatchDog++)) {
        debugger;
        throw new Error("WatchDog");
      }
    } else {
      this._WatchDog = 0;
      this.logicalTime = transaction.id;
    }

    const logEntry: LogEntry = {
      objectPropertyIdentity: this.objectPropertyIdentity,
      logicalTime: this.logicalTime,
      watchDog: this._WatchDog,
      message: 'setValue',
      value: value,
    };
    this._service.addLog(logEntry);
    if (this._report != null) {
      this._report(this, 'set', value);
    }
    if (this._enableReport) {
      this._service.onReport(this, 'set', value);
    }

    try {
      for (const trigger of this._listSinkTrigger) {
        //transaction.addTriggerOrExecute(trigger.trigger, trigger.kind);
        transaction.addTriggerToQueue(trigger.trigger, trigger.kind);
      }
      // if (this.signal != null) {
      //   transaction.addTriggerToQueue(this.signal, "UI");
      // }
      if (this.subject != null) {
        transaction.addTriggerToQueue(this.subject, "UI");
      }
      if (this.sideEffect != null && this.sideEffectTrigger != null) {
        //transaction.addTriggerOrExecute(this.sideEffectTrigger, this.sideEffectTrigger.kind);
        transaction.addTriggerToQueue(this.sideEffectTrigger, this.sideEffectTrigger.kind);
      }
    } finally {
      if (scope == null) {
        transaction.executeTrigger(transaction);
      }
    }
  }

  public asSignal(): Signal<V> {
    return this.signal;
  }

}

class DepDataServiceSubject<V> extends BehaviorSubject<V> implements DepDataPropertyTrigger {
  public name: string | undefined = undefined;
  constructor(
    public readonly property: DepDataProperty<V>
  ) {
    super(property.getValue());
    this.name = `subject-${property.name}`;
  }

  private isDirty: boolean = false;
  setDirty(): boolean {
    if (this.isDirty) {
      return false;
    } else {
      this.isDirty = true;
      return true;
    }
  }

  getDirtyDeps(): number {
    return 0;
  }

  override next(value: V) {
    this.property.setValue(value);
  }

  public executeTrigger(/* scope: DepDataServiceExecutionScope */) {
    try {
      super.next(this.property.getValue());
    } finally {
      this.isDirty = false;
    }
  }
}

class DepDataServiceReadonlySignal<V> implements DepDataPropertyTrigger {
  public name: string | undefined = undefined;
  public writableSignal: WritableSignal<V>;
  private untracked: boolean = false;
  public readonlySignal: Signal<V>;

  constructor(
    public readonly property: DepDataProperty<V>
  ) {
    this.name = `signal-${property.name}`;

    this.writableSignal = signal<V>(this.property.getValue());
    this.readonlySignal = this.writableSignal.asReadonly();
  }

  private isDirty: boolean = false;
  setDirty(): boolean {
    if (this.isDirty) {
      return false;
    } else {
      this.isDirty = true;
      return true;
    }
  }

  getDirtyDeps(): number {
    return 0;
  }

  public executeTrigger() {
    try {
      const value = this.property.getValue();
      // console.log('executeTrigger-signal', this.property.name, value);
      this.writableSignal.set(value);
    } finally {
      this.isDirty = false;
    }
  }
}

export interface IDepDataPropertyDependency<V> {
  name?: string;
  readonly kind: DepDataPropertyTriggerKind;
  readonly sourceProperty: DepDataProperty<V>;
  addSinkTrigger(trigger: DepDataPropertyTrigger): void;
  removeSinkTrigger(trigger: DepDataPropertyTrigger): void;
  getWeight(): number;
  setDirty(): boolean;
  getDirtyDepsNoRecursive(): number;
  executeTrigger(scope: DepDataServiceExecutionScope): void;
}

export class DepDataPropertyDependency<V> implements IDepDataPropertyDependency<V> {
  public name: string | undefined = undefined;

  constructor(
    public readonly kind: DepDataPropertyTriggerKind,
    public readonly sourceProperty: DepDataProperty<V>
  ) {
    this.name = `dependency-${sourceProperty.name}-${kind}`;
  }

  public trigger: DepDataPropertyTrigger | undefined = undefined;

  public addSinkTrigger(trigger: DepDataPropertyTrigger) {
    this.trigger = trigger;
    this.sourceProperty.addSinkTrigger(this, this.kind);
  }

  public removeSinkTrigger(trigger: DepDataPropertyTrigger) {
    if (this.trigger == null) { return; }
    if (!Object.is(trigger, this.trigger)) { return; }
    this.sourceProperty.removeSinkTrigger(this);
    this.trigger = undefined;
  }

  getWeight(): number {
    return this.sourceProperty.weight;
  }

  public setDirty(): boolean {
    if (this.trigger == null) {
      return false;
    }
    if (this.trigger.setDirty != null) {
      if (this.trigger.setDirty()) {
        return true;
      } else {
        return false;
      }
    } else {
      return true;
    }
  }

  getDirtyDepsNoRecursive(): number {
    if (this.sourceProperty.getIsDirty()) {
      return 1;
    } else {
      return 0;
    }
    //return this.sourceProperty.getDirtyDeps();
  }

  getDirtyDeps(): number {
    if (this.sourceProperty.getIsDirty()) {
      return 1; // + this.sourceProperty.getDirtyDeps();
    } else {
      return 0;
    }
  }

  public executeTrigger(scope: DepDataServiceExecutionScope) {
    if (this.trigger == null) { return; }
    this.trigger.executeTrigger(scope);
  }
}

export class DepDataPropertyDependencyGate<V> implements IDepDataPropertyDependency<V>, DepDataPropertyTrigger {
  public name: string | undefined = undefined;
  public readonly kind: DepDataPropertyTriggerKind = "Inner";

  constructor(
    public readonly sourceProperty: DepDataProperty<V>
  ) {
    this.name = `dependency-${sourceProperty.name}-Inner`;
  }

  public trigger: DepDataPropertyTrigger | undefined = undefined;

  public addSinkTrigger(trigger: DepDataPropertyTrigger) {
    this.trigger = trigger;
    this.sourceProperty.addSinkTrigger(this, this.kind);
  }

  public removeSinkTrigger(trigger: DepDataPropertyTrigger) {
    if (this.trigger == null) { return; }
    if (!Object.is(trigger, this.trigger)) { return; }
    this.sourceProperty.removeSinkTrigger(this);
    this.trigger = undefined;
  }

  getWeight(): number {
    return this.sourceProperty.weight;
  }

  public setDirty(): boolean {
    return false;
  }

  getDirtyDepsNoRecursive(): number {
    return 0;
  }

  getDirtyDeps(): number {
    return 0;
  }

  public executeTrigger() { }
}

export class DepDataServiceSource<V, TS> {
  public name: string | undefined = undefined;

  constructor(
    public readonly sourceDependency: SourceDependency<TS>,
    public readonly sourceTransform: SourceTransform<TS, V>,
    public readonly targetProperty: DepDataProperty<V>,
    public readonly service: DepDataService
  ) {
    this.name = `target-${targetProperty.name}`;
    for (const key in sourceDependency) {
      const sd = sourceDependency[key]
      sd.addSinkTrigger(this);
      this.name += `-${sd.sourceProperty.name}`;
    }
  }

  public unsubscribe() {
    for (const key in this.sourceDependency) {
      const sd = this.sourceDependency[key]
      sd.removeSinkTrigger(this);
    }
  }

  weight: number = 0;
  getWeight(): number {
    let result = 0;
    for (const key in this.sourceDependency) {
      const sd = this.sourceDependency[key]
      result += (1 + sd.getWeight());
    }
    this.weight = result;
    return result;
  }


  public updateValue(scope: DepDataServiceExecutionScope): V {
    // this.service.onReport(this.targetProperty, 'updatingValue', this.targetProperty.value);
    const sourceValue: DepDataPropertySourceValue<TS> = {} as DepDataPropertySourceValue<TS>;
    for (const key in this.sourceDependency) {
      const sd = this.sourceDependency[key]
      const value = sd.sourceProperty.getValue();
      sourceValue[key] = value;
    }
    try {
      const currentValue = this.targetProperty.getValue();
      const nextValue = this.sourceTransform(sourceValue, currentValue, scope);
      this.targetProperty.setValue(nextValue);
      return nextValue;
    } catch (error) {
      this.service.onReportError(this.targetProperty as any, 'error', error);
      throw undefined;
    }
  }

  private _isDirty: boolean = false;

  public getIsDirty(): boolean {
    return this._isDirty;
  }

  public setDirty(): boolean {
    if (this._isDirty) {
      return false;
    } else {
      this._isDirty = true;
      return true;
    }
  }

  getDirtyDeps(): number {
    let result = (this._isDirty) ? 1 : 0;
    for (const key in this.sourceDependency) {
      const sd = this.sourceDependency[key]
      result += sd.getDirtyDepsNoRecursive();
    }
    return result;
  }

  public executeTrigger(scope: DepDataServiceExecutionScope) {
    try {
      this.updateValue(scope);
    } catch (error) {
      this.service.onReport<any>(this.targetProperty, 'error', error);
    }
    this._isDirty = false;
  }

  // public valueState: number = 0;

  // public setDirty(): boolean {
  //   if (0 === this.valueState) {
  //     this.valueState = 1;
  //     return true;
  //   }
  //   if (1 === this.valueState) {
  //     return false;
  //   }
  //   if (2 === this.valueState) {
  //     this.valueState = 3;
  //     return false;
  //   }
  //   if (1 === this.valueState) {
  //     return false;
  //   }
  //   throw new Error("Invalid state");
  // }

  // public executeTrigger() {
  //   if (this.valueState === 0) {
  //     throw new Error("not dirty");
  //   } else if (this.valueState === 1) {
  //     // ok
  //   } else if (this.valueState === 2) {
  //     this.valueState = 3;
  //     return;
  //   } else if (this.valueState === 3) {
  //     return;
  //   } else {
  //     throw new Error("Invalid state");
  //   }
  //   this.valueState = 2;
  //   try {
  //     this.updateValue();
  //   } catch (error) {
  //     this.service.onReport<any>(this.targetProperty, 'error', error);
  //   }
  //   if (3 === this.valueState) {
  //     this.service.onReport(this.targetProperty, 'redirty', this.targetProperty.value);
  //   }
  //   this.valueState = 0;
  // }
}

export class DepDataServiceExecutionManager {
  private _listScope: DepDataServiceExecutionScope[] = [];
  private _listTrigger: ListDepDataPropertyTriggerAndKind = new ListDepDataPropertyTriggerAndKind();

  constructor(
    private readonly _service: DepDataService
  ) {
    const root = new DepDataServiceExecutionScope(
      {
        name: "root"
      },
      0,
      true,
      this._listTrigger,
      this,
      this._service);
    this._listScope.push(root);
  }

  public getScope() {
    return this._listScope[this._listScope.length - 1];
  }

  public create(args: DepDataServiceStart | undefined) {
    const previous = this._listScope[this._listScope.length - 1];
    const result = new DepDataServiceExecutionScope(
      args,
      previous.mode,
      false,
      this._listTrigger,
      this,
      this._service);
    this._listScope.push(result);
    return result;
  }

  public remove(scope: DepDataServiceExecutionScope) {
    const index = this._listScope.findIndex(s => s.id === scope.id);
    if (index >= 0) {
      this._listScope.splice(index, 1);
    }
  }
}

export class DepDataServiceExecutionScope {
  private static _nextId = 1;
  public readonly id = DepDataServiceExecutionScope._nextId++;
  public readonly name: string | undefined;
  private readonly _delayedExecution: boolean;
  public readonly mode: number;
  constructor(
    args: DepDataServiceStart | undefined,
    previousMode: number,
    private readonly _root: boolean,
    private _listTrigger: ListDepDataPropertyTriggerAndKind,
    private readonly _manager: DepDataServiceExecutionManager,
    private readonly _Service: DepDataService
  ) {
    this._listTrigger = _listTrigger;
    this.name = args?.name;
    this._listTrigger.name = `scope${this.id}-${this.name}`;
    if (previousMode === 1) {
      this.mode = 2;
    } else if (true === args?.pauseNestedExecution) {
      this.mode = 1;
    } else {
      this.mode = 0;
    }
    this._delayedExecution = args?.delayedExecution ?? false;
  }

  public addTriggerOrExecute(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    // if ("Inner" === kind) {
    //   // console.log('executeOneTrigger', this._listTrigger.name, trigger.name);
    //   this.executeOneTrigger(trigger);
    // } else {
    //   this.addTriggerToQueue(trigger, kind);
    // }
    this.addTriggerToQueue(trigger, kind);
  }

  public addTriggerToQueue(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    // console.log('addTriggerToQueue', this._listTrigger.name, trigger.name);

    if (trigger.setDirty != null) {
      if (trigger.setDirty()) {
        this._listTrigger.add(trigger, kind);
        if (this._root) { this.autoDelayExecuteTrigger(this); }
      } else {
      }
    } else {
      this._listTrigger.add(trigger, kind);
      if (this._root) { this.autoDelayExecuteTrigger(this); }
    }
  }

  public executeTrigger(scope?: DepDataServiceExecutionScope) {
    const scopeUsed = scope ?? this;
    if (this._delayedExecution) {
      this.autoDelayExecuteTrigger(scopeUsed);
    } else {
      this._internalExecuteTrigger(scopeUsed);
    }
  }

  private _internalExecuteTrigger(scope: DepDataServiceExecutionScope) {
    if (this.mode === 2) { return; }
    while (0 < this._listTrigger.length) {
      for (const trigger of this._listTrigger.getPartialListAndClear()) {
        this.executeOneTrigger(trigger, scope);
        // try {
        //   trigger.executeTrigger(scope);
        // } catch (error) {
        //   this._Service.onReportError(this, 'DepDataServiceExecutionScope', error);
        // }
      }
    }
    if (this._root) {
      // do not remove the root scope.
    } else {
      this._manager.remove(this);
    }
  }

  private _autoTrigger = false;
  private autoDelayExecuteTrigger(scope: DepDataServiceExecutionScope) {
    if (this._autoTrigger) { return; }
    this._autoTrigger = true;
    window.requestAnimationFrame(() => {
      this._autoTrigger = false;
      this._internalExecuteTrigger(scope);
    });
  }

  private executeOneTrigger(trigger: DepDataPropertyTrigger, scope: DepDataServiceExecutionScope) {
    try {
      trigger.executeTrigger(scope);
    } catch (error) {
      this._Service.onReportError(this, 'DepDataServiceExecutionScope', error);
    }
  }
}

export type DepDataSideEffectTriggerArguments<V> = {
  fn: (value: V) => void;
  kind?: DepDataPropertyTriggerKind;
  requestAnimationFrame?: boolean;
};

export class DepDataSideEffectTrigger<V> implements DepDataPropertyTrigger {
  public name: string | undefined = undefined;

  constructor(
    public sourceProperty: DepDataProperty<V>,
    private args: DepDataSideEffectTriggerArguments<V>,
    public kind: DepDataPropertyTriggerKind
  ) {
    this.name = `sideEffect-${sourceProperty.name}`;
  }

  private isDirty: boolean = false;
  setDirty(): boolean {
    if (this.isDirty) {
      return false;
    } else {
      this.isDirty = true;
      return true;
    }
  }

  getDirtyDeps(): number {
    return 0;
  }

  executeTrigger(scope: DepDataServiceExecutionScope) {
    if (true === this.args.requestAnimationFrame) {
      window.requestAnimationFrame(() => {
        try {
          const value = this.sourceProperty.getValue();
          this.args.fn(value);
        } finally {
          this.isDirty = false;
        }
      });
    } else {
      try {
        const value = this.sourceProperty.getValue();
        this.args.fn(value);
      } finally {
        this.isDirty = true;
      }
    }
  }
};

export class ListDepDataPropertyTriggerAndKind {
  private _listTrigger: DepDataPropertyTriggerAndKind[] = [];
  public name: string | undefined = undefined;

  constructor() { }
  public [Symbol.iterator](): Iterator<DepDataPropertyTriggerAndKind> {
    let current = -1;
    return {
      next: () => {
        ++current;
        if (current < this._listTrigger.length) {
          const value: DepDataPropertyTriggerAndKind = this._listTrigger[current];
          return ({ value: value, done: false });
        } else {
          return ({ value: undefined, done: true });
        }
      }
    };
  }

  public add(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    this._listTrigger.push({ trigger: trigger, kind: kind });
  }

  public remove(trigger: DepDataPropertyTrigger) {
    const index = this._listTrigger.findIndex(t => Object.is(t, trigger));
    if (index >= 0) {
      this._listTrigger.splice(index, 1);
      return true;
    } else {
      return false;
    }
  }

  public get length() {
    return this._listTrigger.length;
  }

  public getPartialListAndClear(): DepDataPropertyTrigger[] {
    if (0 === this._listTrigger.length) {
      return [] as DepDataPropertyTrigger[];
    } else {
      const result = this._listTrigger.splice(0, this._listTrigger.length);
      return result.map(item => item.trigger);
    }
  }

  public getNext(): (DepDataPropertyTrigger | undefined) {
    if (0 === this._listTrigger.length) {
      return undefined;
    } else {
      let index: number = 0;
      if (1 < this._listTrigger.length) {
        const trigger = this._listTrigger[index].trigger;
        let indexDirtyDeps = trigger.getDirtyDeps();
        if (0 === indexDirtyDeps) {
          // use this index
        } else {
          for (let betterIndex = 1; betterIndex < this._listTrigger.length; betterIndex++) {
            const betterTrigger = this._listTrigger[betterIndex].trigger;
            let betterDirtyDeps = betterTrigger.getDirtyDeps();
            if (0 === betterDirtyDeps) {
              this._listTrigger.splice(betterIndex, 1);
              return betterTrigger;
            } else if (betterDirtyDeps < indexDirtyDeps) {
              betterDirtyDeps = indexDirtyDeps;
            }
          }
        }
      }
      return this._listTrigger.splice(index, 1)[0].trigger;
    }
  }
  public clear(): void {
    if (0 === this._listTrigger.length) {
      this._listTrigger.length = 0;
    }
  }
}
/*
export class ListDepDataPropertyTriggerAndKind {
  private _listTrigger: DepDataPropertyTrigger[] = [];
  private _CountInner = 0;
  private _CountPublic = 0;
  private _CountUi = 0;
  name: string | undefined = undefined;

  constructor() { }

  public [Symbol.iterator](): Iterator<DepDataPropertyTriggerAndKind> {
    let current = 0;
    return {
      next: () => {
        if (current < this._listTrigger.length) {
          const trigger: DepDataPropertyTrigger = this._listTrigger[current];
          const kind: DepDataPropertyTriggerKind = (current < this._CountInner) ? "Inner"
            : (current < this._CountInner + this._CountPublic) ? "Public"
              : "UI";
          const value: DepDataPropertyTriggerAndKind = { trigger: trigger, kind: kind };
          current++;
          return ({ value: value, done: false });
        } else {
          return ({ value: undefined, done: true });
        }
      }
    };
  }

  public add(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    if ("Inner" === kind) {
      this._listTrigger.splice(this._CountInner, 0, trigger);
      this._CountInner++;
    } else if ("Public" === kind) {
      this._listTrigger.splice(this._CountInner + this._CountPublic, 0, trigger);
      this._CountPublic++;
    } else {
      this._listTrigger.push(trigger);
      this._CountUi++;
    }
  }

  public remove(trigger: DepDataPropertyTrigger) {
    const index = this._listTrigger.findIndex(t => Object.is(t, trigger));
    if (index >= 0) {
      this._listTrigger.splice(index, 1);
      return true;
    } else {
      return false;
    }
  }

  public get length() {
    return this._listTrigger.length;
  }

  public getPartialListAndClear() {
    // console.log('getPartialListAndClear', this.name, this._CountInner, this._CountPublic, this._CountUi, this._listTrigger.length);
    if (0 < this._CountInner) {
      const result = this._listTrigger.slice(0, this._CountInner);
      this._listTrigger.splice(0, this._CountInner);
      this._CountInner = 0;
      return result;
    }
    if (0 < this._CountPublic) {
      const result = this._listTrigger.slice(0, this._CountPublic);
      this._listTrigger.splice(0, this._CountPublic);
      this._CountPublic = 0;
      return result;
    }
    if (0 < this._CountUi) {
      const result = this._listTrigger.slice(0, this._CountUi);
      this._listTrigger.splice(0, this._CountUi);
      this._CountUi = 0;
      return result;
    }
    if (0 < this._listTrigger.length) {
      const result = this._listTrigger.slice();
      this._listTrigger.length = 0;
      return result;
    }
    {
      return [];
    }
  }

  public getListAndClear() {
    const result = this._listTrigger;
    this._listTrigger = [];
    this._CountInner = 0;
    this._CountPublic = 0;
    this._CountUi = 0;
    return result;
  }

  public clear() {
    this._listTrigger.length = 0;
    this._CountInner = 0;
    this._CountPublic = 0;
    this._CountUi = 0;
  }
}
*/
