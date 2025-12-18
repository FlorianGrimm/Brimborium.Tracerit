import { effect, Injectable, Signal, signal, untracked, WritableSignal } from '@angular/core';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { BehaviorSubject, InteropObservable, Subscribable, Subscription } from 'rxjs';

export type DepDataServiceEnd = (fnFinish: undefined | (() => void)) => void;

export type TransformFN<V> = (value: V) => V;
export type ReportFN<V> = ((property: DepDataProperty<V>, message: string, value: V) => void);
export type ReportErrorFN = ((that: any, classMethod: string, error: unknown) => void);

export type DepDataPropertySourceDependency<TS> = {
  [name in keyof TS]: DepDataPropertyDependency<TS[name]>;
}
export type DepDataPropertySourceValue<TS> = {
  [name in keyof TS]: TS[name];
}

export type DepDataPropertyArguments<V> = {
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

  /* report is called after the value is set - before the dependcies and sideEffect is called. */
  report?: ReportFN<V> | undefined;

  /* subscription is used to unsubscribe all dependcies and sideEffect. */
  subscription: Subscription;
};

export type DepDataPropertyInputArguments<V> = {
  input: Signal<V>;
}

export type DepDataPropertyInputAndTransformArguments<S, V> = {
  input: Signal<S>;
  transform: (value: S) => V;
}

export type DepDataPropertyTrigger = {
  name: string | undefined;
  setDirty?: (() => boolean);
  executeTrigger(): void;
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

@Injectable({
  providedIn: 'root',
})
export class DepDataService {

  private _PropertyIndex = 1;
  constructor() { }

  /** creates a new initializer. */
  public createInitializer() {
    return new DepDataPropertyInitializer();
  }

  /** creates a new property. */
  public createProperty<V>(
    args: DepDataPropertyArguments<V>
  ): DepDataProperty<V> {
    return new DepDataProperty<V>(args, this, this._PropertyIndex++);
  }

  private readonly _ExecutionScope = new DepDataServiceExecutionManager(this);

  /** starts a new transaction. */
  public start(args: DepDataServiceStart) {
    return this._ExecutionScope.create(args);
  }

  /** custom callback for onReport. */
  public report: ReportFN<any> | undefined;

  /** calls the custom callback for onReport or the default one. */
  public onReport<V>(property: DepDataProperty<V>, message: string, value: V) {
    if (this.report) {
      this.report(property, message, value);
    } else {
      if (value === undefined) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, undefined);
      } else if (value === null) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, null);
      } else if (Array.isArray(value)) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, value.length);
      } else if (value instanceof ZonedDateTime) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, value.toString());
      } else if (value instanceof Duration) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, value.toString());
      } else if (value instanceof Date) {
        console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, value.toISOString());
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
            console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, valueToString);
            return;
          }
        }
        {
          console.log(`Property ${property.name}-${property.propertyIndex}-${property.logicalTime} ${message}`, value);
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
}

/** allows to delay the creation of a property *source*. */
export class DepDataPropertyInitializer {
  private _ListDelayed: DepDataPropertyWithSourceDelayed<any, any>[] | undefined = undefined;

  constructor() { }

  /** internal - add a delayed creation. */
  public add<TS>(delayed: DepDataPropertyWithSourceDelayed<any, any>) {
    const list = (this._ListDelayed ??= []);
    list.push(delayed);
  }

  /** internal - execute all delayed creations. */
  public execute() {
    const listDelayed = this._ListDelayed;
    this._ListDelayed = undefined;
    if (listDelayed == null) { return; }
    for (const delayed of listDelayed) {
      delayed.fnDelayed(delayed);
    }
  }
}

/** arguments for withSource. */
export type DepDataPropertyWithSource<TS, V> = {
  sourceDependency: DepDataPropertySourceDependency<TS>;
  sourceTransform: (value: DepDataPropertySourceValue<TS>) => V;
  subscription?: Subscription;
  depDataPropertyInitializer?: DepDataPropertyInitializer;
}

/** arguments for withSource with delayed execution. */
export type DepDataPropertyWithSourceDelayed<TS, V> =
  DepDataPropertyWithSource<TS, V>
  & {
    fnDelayed: (args: DepDataPropertyWithSourceDelayed<TS, V>) => void;
  }

/** property - with a value if set the 
 * value can be transformed
 * and can be compared with the old value (and skipped if equal)
 * and the value is set.
 * Than the dependencies are recalculated and the sideEffects are called. 
 */
export class DepDataProperty<V> implements InteropObservable<V> {
  public readonly name: string;
  public value: V;
  public logicalTime: number = 0;
  public subscription: Subscription;
  private _transform: ((value: V) => V) | undefined;
  private _compare: ((a: V, b: V) => boolean) | undefined;
  private _report: ReportFN<V> | undefined;
  private _listSinkTrigger: ListDepDataPropertyTriggerAndKind = new ListDepDataPropertyTriggerAndKind();
  private sideEffect: DepDataSideEffectTriggerArguments<V> | undefined;

  constructor(
    args: DepDataPropertyArguments<V>,
    private _service: DepDataService,
    public readonly propertyIndex: number
  ) {
    this.name = args.name;
    this._transform = args.transform;
    this._compare = args.compare;
    this._report = args.report;
    this.subscription = args.subscription;
    this.value = args.initialValue;
    this.sideEffect = args.sideEffect;
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
            const valueV = transform(valueS);
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

  [Symbol.observable](): Subscribable<V> {
    return this.asObserable();
  }

  public getValue(): V {
    return this.value;
  }

  private _WatchDog: number = 0;

  public setValue(value: V) {
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
    const transaction = this._service.start({
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

    if (this._report != null) {
      this._report(this, 'set', value);
    }
    this._service.onReport(this, 'set', value);

    try {
      for (const trigger of this._listSinkTrigger) {
        transaction.addTriggerOrExecute(trigger.trigger, trigger.kind);
      }
      if (this.signal != null) {
        transaction.addTriggerToQueue(this.signal, "UI");
      }
      if (this.subject != null) {
        transaction.addTriggerToQueue(this.subject, "UI");
      }
      if (this.sideEffect != null) {
        transaction.addTriggerOrExecute(new DepDataSideEffectTrigger<V>(this.sideEffect, value), this.sideEffect.kind ?? "UI");
      }
    } finally {
      transaction.executeTrigger();
    }
  }

  public fireTrigger() {
    if (this.closed) {
      this._service.onReportError(this, 'DepDataProperty.setValue', new Error("closed"));
      return;
    }

    const value = this.value;
    const transaction = this._service.start({
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

    if (this._report != null) {
      this._report(this, 'fireTrigger', value);
    }
    this._service.onReport(this, 'fireTrigger', value);

    try {
      for (const trigger of this._listSinkTrigger) {
        transaction.addTriggerOrExecute(trigger.trigger, trigger.kind);
      }
      if (this.signal != null) {
        transaction.addTriggerToQueue(this.signal, "UI");
      }
      if (this.subject != null) {
        transaction.addTriggerToQueue(this.subject, "UI");
      }
      if (this.sideEffect != null) {
        transaction.addTriggerOrExecute(new DepDataSideEffectTrigger<V>(this.sideEffect, value), this.sideEffect.kind ?? "UI");
      }
    } finally {
      transaction.executeTrigger();
    }
  }

  /** use this as a dependency. */
  public dependencyInner(): DepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("Inner", this); };

  /** use this as a dependency. */
  public dependencyPublic(): DepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("Public", this); };

  /** use this as a dependency. */
  public dependencyUi(): DepDataPropertyDependency<V> { return new DepDataPropertyDependency<V>("UI", this); };

  private listSource: DepDataServiceSource<V, any>[] | undefined = undefined;

  public withSource<TS>(
    args: DepDataPropertyWithSource<TS, V>
  ) {
    if (args.depDataPropertyInitializer == null) {
      this._internalWithSource(args);
    } else {
      args.depDataPropertyInitializer.add({
        ...args,
        fnDelayed: this._internalWithSource.bind(this),
      });
    }
    return this;
  }

  public withSourceIdentity(
    dependency: DepDataPropertyDependency<V>,
    depDataPropertyInitializer?: DepDataPropertyInitializer
  ) {
    return this.withSource<{ value: V }>({
      sourceDependency: { value: dependency },
      sourceTransform: (d) => d.value,
      depDataPropertyInitializer: depDataPropertyInitializer,
    });
  }

  private _internalWithSource<TS>(
    args: DepDataPropertyWithSource<TS, V>
  ) {
    let subscription = args.subscription;
    if (subscription == null) {
      subscription = this.subscription;
    } else {
      this.subscription.add(subscription);
    }
    const source = new DepDataServiceSource<V, TS>(args.sourceDependency, args.sourceTransform, this, this._service);
    subscription.add(source);

    const listSource = (this.listSource ??= []);
    listSource.push(source);
    source.updateValue();

    return this;
  }

  unsubscribe(): void {
    const listSource = this.listSource;
    this.listSource = undefined;
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

  private signal: DepDataServiceSignal<V> | undefined;
  public asSignal(): Signal<V> {
    const signal = this.signal ??= new DepDataServiceSignal<V>(this);
    return signal.readonlySignal;
  }

  private subject: DepDataServiceSubject<V> | undefined;
  public asObserable(): BehaviorSubject<V> {
    const subject = this.subject ??= new DepDataServiceSubject<V>(this);
    return subject.subject;
  }

  _getListSinkTrigger(): { trigger: DepDataPropertyTrigger, kind: string }[] {
    return Array.from(this._listSinkTrigger);
  }
}

export class DepDataServiceSubject<V> {
  public name: string | undefined = undefined;
  public subject: BehaviorSubject<V>;

  constructor(
    public readonly property: DepDataProperty<V>
  ) {
    this.name = `subject-${property.name}`;
    this.subject = new BehaviorSubject<V>(this.property.getValue());
  }
  public executeTrigger() {
    this.subject.next(this.property.getValue());
  }
}

export class DepDataServiceSignal<V> {
  public name: string | undefined = undefined;
  public writableSignal: WritableSignal<V>;
  public readonlySignal: Signal<V>;
  constructor(
    public readonly property: DepDataProperty<V>
  ) {
    this.name = `signal-${property.name}`;

    this.writableSignal = signal<V>(this.property.getValue());
    this.readonlySignal = this.writableSignal.asReadonly();
  }

  public executeTrigger() {
    const value = this.property.getValue();
    // console.log('executeTrigger-signal', this.property.name, value);
    this.writableSignal.set(value);
  }
}

export class DepDataPropertyDependency<V> {
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

  //public isDirty: boolean = false;

  public setDirty(): boolean {
    // if (this.isDirty) {
    //   return false;
    // }
    if (this.trigger == null) {
      return false;
    }
    if (this.trigger.setDirty != null) {
      if (this.trigger.setDirty()) {
        //this.isDirty = true;
        return true;
      } else {
        return false;
      }
    } else {
      //this.isDirty = true;
      return true;
    }
  }
  public executeTrigger() {
    // if (this.isDirty) {
    //   this.isDirty = false;
    //   if (this.trigger == null) { return; }
    //   this.trigger.executeTrigger();
    // }
    if (this.trigger == null) { return; }
    this.trigger.executeTrigger();
  }
}

export class DepDataServiceSource<V, TS> {
  public name: string | undefined = undefined;

  constructor(
    public readonly sourceDependency: DepDataPropertySourceDependency<TS>,
    public readonly sourceTransform: (value: DepDataPropertySourceValue<TS>) => V,
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

  public updateValue(): V {
    // this.service.onReport(this.targetProperty, 'updatingValue', this.targetProperty.value);
    const sourceValue: DepDataPropertySourceValue<TS> = {} as DepDataPropertySourceValue<TS>;
    for (const key in this.sourceDependency) {
      const sd = this.sourceDependency[key]
      const value = sd.sourceProperty.getValue();
      sourceValue[key] = value;
    }
    const result = this.sourceTransform(sourceValue);
    this.targetProperty.setValue(result);
    return result;
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

  public executeTrigger() {
    this._isDirty = false;
    try {
      this.updateValue();
    } catch (error) {
      this.service.onReport<any>(this.targetProperty, 'error', error);
    }
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
    if ("Inner" === kind) {
      console.log('executeOneTrigger', this._listTrigger.name, trigger.name);
      this.executeOneTrigger(trigger);
    } else {
      this.addTriggerToQueue(trigger, kind);
    }
  }

  public addTriggerToQueue(trigger: DepDataPropertyTrigger, kind: DepDataPropertyTriggerKind) {
    console.log('addTriggerToQueue', this._listTrigger.name, trigger.name);

    if (trigger.setDirty != null) {
      if (trigger.setDirty()) {
        this._listTrigger.add(trigger, kind);
        if (this._root) { this.autoDelayExecuteTrigger(); }
      } else {
      }
    } else {
      this._listTrigger.add(trigger, kind);
      if (this._root) { this.autoDelayExecuteTrigger(); }
    }
  }

  public executeTrigger() {
    if (this._delayedExecution) {
      this.autoDelayExecuteTrigger();
    } else {
      this._internalExecuteTrigger();
    }
  }

  private _internalExecuteTrigger() {
    if (this.mode === 2) { return; }
    while (0 < this._listTrigger.length) {
      for (const trigger of this._listTrigger.getPartialListAndClear()) {
        try {
          trigger.executeTrigger();
        } catch (error) {
          this._Service.onReportError(this, 'DepDataServiceExecutionScope', error);
        }
      }
    }
    if (this._root) {
      // do not remove the root scope.
    } else {
      this._manager.remove(this);
    }
  }

  private _autoTrigger = false;
  private autoDelayExecuteTrigger() {
    if (this._autoTrigger) { return; }
    this._autoTrigger = true;
    window.requestAnimationFrame(() => {
      this._autoTrigger = false;
      this._internalExecuteTrigger();
    });
  }

  private executeOneTrigger(trigger: DepDataPropertyTrigger) {
    try {
      trigger.executeTrigger();
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

export class DepDataSideEffectTrigger<V> {
  public name: string | undefined = undefined;

  constructor(
    private args: DepDataSideEffectTriggerArguments<V>,
    private value: V
  ) {
    this.name = `sideEffect-${args.fn.name}`;
  }

  executeTrigger() {
    if (true === this.args.requestAnimationFrame) {
      window.requestAnimationFrame(() => {
        try {
          this.args.fn(this.value);
        } catch {
        }
      });
    } else {
      try {
        this.args.fn(this.value);
      } catch {
      }
    }
  }

};

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
    console.log('getPartialListAndClear', this.name, this._CountInner, this._CountPublic, this._CountUi, this._listTrigger.length);
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
