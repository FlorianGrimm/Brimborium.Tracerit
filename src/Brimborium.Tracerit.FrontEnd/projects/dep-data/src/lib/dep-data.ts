import {
  effect,
  EffectRef,
  Injectable,
  isSignal,
  OnDestroy,
  OnInit,
  Signal,
  untracked,
  WritableSignal
} from '@angular/core';
// import { Duration, ZonedDateTime } from '@js-joda/core';
import { Subscription, Unsubscribable } from 'rxjs';

export type AnyIdentitfier = {
  readonly objectName?: string;
  readonly objectIndex?: number;
  readonly propertyName?: string;
  readonly propertyIndex?: number;
  readonly fullName: string;


};
export enum ReportLogLevel { trace, info, warn, error, disabled };
export type ReportFN<V> = ((logLevel: ReportLogLevel, sender: AnyIdentitfier, message: string, logicalTime: number, value: V) => void);
export type ReportErrorFN = ((logLevel: ReportLogLevel, sender: AnyIdentitfier, classMethod: string, error: unknown) => void);
export type ReportConvertLogValueFN<V = any> = (value: V, maxLevel: number) => any;
export type ReportConvertObjectLogValueFN<V = any> = (value: V, maxLevel: number) => ({ converted: true, result: any } | { converted: false, result?: undefined });
export type LogEntry = {
  logLevel: ReportLogLevel;
  sender: AnyIdentitfier,
  logicalTime: number;
  message: string;
  value: any;
};

@Injectable({
  providedIn: 'root',
})
export class DepDataService {
  public readonly depIdentityService = new DepIdentityService();
  public readonly objectIdentifier: ObjectIdentifier;
  public readonly mapReportConvertLogValue = new Map<string, ReportConvertLogValueFN>();
  public readonly listReportConvertLogValue: ReportConvertObjectLogValueFN[] = [];
  constructor() {
    this.objectIdentifier = this.depIdentityService.createDepDataServiceIdentity();
    // this.mapReportConvertLogValue.set("ZonedDateTime", (value: ZonedDateTime, maxLevel: number) => value.toString())
    // this.mapReportConvertLogValue.set("Duration", (value: Duration, maxLevel: number) => value.toString())
    this.mapReportConvertLogValue.set("Number", (value: number, maxLevel: number) => value)
    this.mapReportConvertLogValue.set("String", (value: string, maxLevel: number) => value)
    this.mapReportConvertLogValue.set("Boolean", (value: any[], maxLevel: number) => value)
    this.mapReportConvertLogValue.set("Date", (value: any[], maxLevel: number) => value)
    this.mapReportConvertLogValue.set("Array", (value: any[], maxLevel: number) => value.length)
  }

  public wrap(
    that: DepDataPropertyEnhancedThat
  ) {
    return new DepDataObject(that, this, this.depIdentityService);
  }

  private haltMessage: string | undefined = undefined;
  halt(message: string) {
    this.haltMessage = message;
  }
  throwIfHalted() {
    if (this.haltMessage != null) { throw new Error(this.haltMessage); }
  }

  listScopes: DepDataServiceExecutionScope[] = [];
  startScope(): DepDataServiceExecutionScope {
    this.throwIfHalted();

    const result = new DepDataServiceExecutionScope(this, this.depIdentityService);
    this.listScopes.push(result);
    this.log(ReportLogLevel.trace, result.objectIdentifier, "startScope", result.scopeIndex, undefined);
    return result;
  }

  removeScope(scope: DepDataServiceExecutionScope) {
    const listScopesLength = this.listScopes.length;
    if (0 < listScopesLength) {
      const foundScope = this.listScopes[listScopesLength - 1];
      if (Object.is(scope, foundScope)) {
        this.listScopes.splice(listScopesLength - 1, 1);
        this.log(ReportLogLevel.trace, scope.objectIdentifier, "removeScope:last", scope.scopeIndex, { index: listScopesLength - 1 });
        return;
      }
    }
    {
      const foundIndex = this.listScopes.findIndex(item => Object.is(item, scope));
      if (0 <= foundIndex) {
        this.listScopes.splice(foundIndex, 1);
        this.log(ReportLogLevel.trace, scope.objectIdentifier, "removeScope:inner", scope.scopeIndex, { index: foundIndex });
        return;
      }
    }

    this.log(ReportLogLevel.trace, scope.objectIdentifier, "removeScope:not found", scope.scopeIndex, undefined);
  }

  public logLevel: ReportLogLevel = ReportLogLevel.disabled;

  /** custom callback for onReport. */
  public loggerFN: ReportFN<any> | undefined;

  /** calls the custom callback for onReport or the default one. */
  public log<V>(logLevel: ReportLogLevel, sender: { fullName: string }, message: string, logicalTime: number, value: V) {
    if (this.loggerFN) {
      if (ReportLogLevel.disabled === logLevel) { return; }
      if (logLevel < this.logLevel) { return; }
      this.loggerFN(logLevel, sender, message, logicalTime, value);
    } else {
      if (ReportLogLevel.disabled === logLevel) { return; }
      if (logLevel < this.logLevel) { return; }
      // TODO: generalize/enable customize
      let fnLog: ((...data: any[]) => void);
      switch (logLevel) {
        case ReportLogLevel.error: fnLog = console.error; break;
        case ReportLogLevel.info: fnLog = console.info; break;
        case ReportLogLevel.warn: fnLog = console.warn; break;
        case ReportLogLevel.trace: fnLog = console.trace; break;
        default: fnLog = console.log; break;
      }
      const logValue = this.convertLogValue(value, 2);
      fnLog(sender.fullName, message, logicalTime, logValue);
    }
  }

  convertLogValue<V>(value: V, maxlevel: number): any {
    if (value == null) { return value; }
    if (Array.isArray(value)) { return value.length; }
    switch (typeof value) {
      case 'bigint':
      case 'boolean':
      case 'number':
      case 'string': return value;
      case 'symbol': return value.toString();
      case 'function': return value.name;
      default: break;
    }
    if ('object' === typeof value) {
      const constructorName = value.constructor.name;

      if (maxlevel <= 0) {
        return constructorName;
      } else {
        if ('Object' === constructorName) {
          for (const convertFN of this.listReportConvertLogValue) {
            const { converted, result } = convertFN(value, maxlevel - 1);
            if (converted) { return result; }
          }
        } else {
          const convertFN = this.mapReportConvertLogValue.get(constructorName);
          if (convertFN != null) { return convertFN(value, maxlevel - 1); }
        }

        {
          const result: any = {};
          let keyWatchdog = 10;
          for (const key in value) {
            const valueC = this.convertLogValue(value[key], maxlevel - 1);
            if (valueC != null) {
              result[key] = valueC;
              if ((keyWatchdog--) <= 0) {
                return result;
              }
            }
          }
          return result;
        }
      }
    }
    return value;
  }


  /** custom callback for onReportError. */
  public reportError: ReportErrorFN | undefined;

  /** calls the custom callback for onReportError or the default one. */
  public onReportError(logLevel: ReportLogLevel, that: any, classMethod: string, error: unknown) {
    if (this.reportError) {
      this.reportError(logLevel, that, classMethod, error);
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

  ListLog: LogEntry[] = [];
  addLog(logEntry: LogEntry) {
    if (!this._isLoggingEnabled) { return; }
    if (1000 < this.ListLog.length) {
      this.ListLog.splice(0, 500);
    }
    this.ListLog.push(logEntry);
    // console.log({ ...logEntry.objectPropertyIdentity, ...logEntry });
  }

  setupLoggerFnToListLog(
    logLevel: ReportLogLevel | undefined
  ) {
    if (undefined === logLevel) {
    } else {
      this.logLevel = logLevel;
    }
    this.loggerFN = ((logLevel: ReportLogLevel, sender: AnyIdentitfier, message: string, logicalTime: number, value: any) => {
      this.ListLog.push({ logLevel: logLevel, sender: sender, message: message, logicalTime: logicalTime, value: value });
    });
  }

}

export class DepIdentityService {
  private static _DepDataServiceIndex = 1;
  public nextDepDataServiceIndex() {
    return DepIdentityService._DepDataServiceIndex++;
  }

  createDepDataServiceIdentity(): ObjectIdentifier {
    const objectIndex = this.nextDepDataServiceIndex();
    const result: ObjectIdentifier = Object.freeze({
      fullName: `DepDataService-${objectIndex}`,
      objectName: 'DepDataService',
      objectIndex: objectIndex,
    });
    return result;
  }

  private _ObjectIndex = 1;
  public nextObjectIndex() {
    return this._ObjectIndex++;
  }

  createObjectIdentity(name: string): ObjectIdentifier {
    const objectIndex = this.nextObjectIndex();
    const result: ObjectIdentifier = Object.freeze({
      fullName: `${name}-${objectIndex}`,
      objectName: name,
      objectIndex: objectIndex,
    });
    return result;
  }

  private _PropertyIndex = 1;
  public nextPropertyIndex() {
    return this._PropertyIndex++;
  }

  createPropertyIdentity(objectIdentifier: ObjectIdentifier, name: string): ObjectPropertyIdentity {
    const propertyIndex = this.nextPropertyIndex();
    const result: ObjectPropertyIdentity = Object.freeze({
      fullName: `${objectIdentifier.fullName}-${name}-${propertyIndex}`,
      objectName: objectIdentifier.objectName,
      objectIndex: objectIdentifier.objectIndex,
      propertyName: name,
      propertyIndex: propertyIndex,
    });
    return result;
  }

  private _ScopeIndex = 1;
  public nextScopeIndex() {
    return this._ScopeIndex++;
  }

  createScopeIdentity(objectIndex?: number): ObjectIdentifier {
    if (objectIndex == null) {
      objectIndex = this.nextScopeIndex();
    }
    const result: ObjectIdentifier = Object.freeze({
      fullName: `scope-${objectIndex}`,
      objectName: "scope",
      objectIndex: objectIndex,
    });
    return result;
  }
}

export type DepDataPropertyEnhancedThat = Partial<OnDestroy>
  & Partial<OnInit>
  & Partial<{
    depDataPropertyInitializer: DepDataObject;
    subscription: Subscription;
  }>;

export type DepDataPropertyBaseArgument<V> = {
  name?: string;
  equal?: (a: V, b: V) => boolean;
};

export type DepDataPropertyValueArgument<V> = DepDataPropertyBaseArgument<V>
  & {
    initialValue: V;
  };

export type DepDataPropertyWrapSignalArgument<V> = DepDataPropertyBaseArgument<V>
  & {};

export type ObjectIdentifier = {
  readonly objectName: string;
  readonly objectIndex: number;
  readonly fullName: string;
}

export type ObjectPropertyIdentity = {
  readonly objectName: string;
  readonly objectIndex: number;
  readonly propertyName: string;
  readonly propertyIndex: number;
  readonly fullName: string;
}

export class DepDataObject implements Unsubscribable {
  public readonly objectIdentifier: ObjectIdentifier;
  public readonly depDataService: DepDataService;
  public readonly depIdentityService: DepIdentityService
  public readonly subscription: Subscription;

  constructor(
    that: DepDataPropertyEnhancedThat,
    depDataService: DepDataService,
    depIdentityService: DepIdentityService
  ) {
    this.objectIdentifier = depIdentityService.createObjectIdentity(that.constructor.name);
    this.depDataService = depDataService;
    this.depIdentityService = depIdentityService;
    this.subscription = that.subscription ?? new Subscription();
    if (that.ngOnInit == null) {
      that.ngOnInit = () => {
        this.initialize();
      };
    }
    if (that.ngOnDestroy == null) {
      that.ngOnDestroy = () => {
        this.destroy();
      };
    }
    this.subscription.add(this);
  }

  destroy(): void {
    this.subscription.unsubscribe();
  }

  unsubscribe(): void {
    // HERE
    this._closed = true;
  }
  private _closed: boolean = false;
  public get closed(): boolean { return this._closed; }

  public createProperty<V>(
    args: DepDataPropertyValueArgument<V>
  ): DepDataPropertyValue<V> {
    const result = new DepDataPropertyValue<V>(args, this, this.depIdentityService);

    if (this.listDepDataPropertyInitilizer == null) {
      this.listDepDataPropertyInitilizer = [];
    }
    this.listDepDataPropertyInitilizer.push(result);

    return result;
  }
  public createPropertyWrapSignal<V>(
    signal: WritableSignal<V>,
    args?: DepDataPropertyWrapSignalArgument<V> | undefined
  ): DepDataPropertyWrapSignal<V> {
    const result = new DepDataPropertyWrapSignal<V>(signal, args, this, this.depIdentityService);

    if (this.listDepDataPropertyInitilizer == null) {
      this.listDepDataPropertyInitilizer = [];
      window.requestAnimationFrame(() => { this.ensureInitializeIsCalled(); });
    }
    this.listDepDataPropertyInitilizer.push(result);

    return result;
  }

  public createPropertyWrapWritableSignal<V>(
    signal: WritableSignal<V>,
    args?: DepDataPropertyWrapSignalArgument<V> | undefined
  ): DepDataPropertyWrapWritableSignal<V> {
    const result = new DepDataPropertyWrapWritableSignal<V>(signal, args, this, this.depIdentityService);

    if (this.listDepDataPropertyInitilizer == null) {
      this.listDepDataPropertyInitilizer = [];
      window.requestAnimationFrame(() => { this.ensureInitializeIsCalled(); });
    }
    this.listDepDataPropertyInitilizer.push(result);

    return result;
  }

  public listDepDataPropertyInitilizer: (IDepDataPropertyInitilizer[] | undefined) = undefined;

  public initialize() {
    const listDepDataPropertyInitilizer = this.listDepDataPropertyInitilizer;
    this.listDepDataPropertyInitilizer = undefined;
    if (listDepDataPropertyInitilizer != null) {
      const scope = this.depDataService.startScope();
      for (const depDataPropertyInitilizer of listDepDataPropertyInitilizer) {
        depDataPropertyInitilizer.initilize(scope);
      }
      scope.commit();
    }
  }

  private ensureInitializeIsCalled() {
    if (this.listDepDataPropertyInitilizer == null) {
      const message = `${this.objectIdentifier.fullName} initialize() is not called.`;
      this.depDataService.halt(message);
      throw new Error(message);
    }
  }

}

export type DepDataSourceValue<V> = IDepDataProperty<V> | Signal<V>;

export type DepDataPropertySourceDependency<TS> = {
  [name in keyof TS]: DepDataSourceValue<TS[name]>;
};
export type DepDataPropertySourceDependencyProperty<TS> = {
  [name in keyof TS]: IDepDataProperty<TS[name]>;
};

export type DepDataPropertySourceValue<TS> = {
  [name in keyof TS]: TS[name];
};

export type SourceDependency<TS> = DepDataPropertySourceDependency<TS>;
export type SourceTransform<TS, V> = (value: DepDataPropertySourceValue<TS>, currentValue: V, scope: DepDataServiceExecutionScope) => V;
export type DepDataSourceArguments<TS, V> = {
  sourceDependency: SourceDependency<TS>;
  sourceTransform: SourceTransform<TS, V>;
};

export type DepDataEffectArguments<V> = {
  sideEffect: (value: V) => void;
  animationFrame?: boolean;
}

export interface IDepDataProperty<V> {
  readonly objectPropertyIdentifier: ObjectPropertyIdentity;

  getValue(): V;
  setValue(value: V, scope?: DepDataServiceExecutionScope): void;

  addDependencySource(source: IDepDataDependencySource): void;
  removeDependencySource(source: IDepDataDependencySource): void;

  setDirty(): boolean;
  getDirtyWeight(maxlevel: number): number;
  resetDirty(): void;
};

export interface IDepDataPropertyInitilizer {
  initilize(scope: DepDataServiceExecutionScope): void;
};

export interface IDepDataEffect {
  execute(): void;
}

export class DepDataPropertyBase<V> implements IDepDataProperty<V>, IDepDataPropertyInitilizer, Unsubscribable {
  readonly objectPropertyIdentifier: ObjectPropertyIdentity;
  readonly equal: (a: V, b: V) => boolean;
  readonly depThis: DepDataObject;
  readonly depDataService: DepDataService;
  valueVersion: number;

  constructor(
    args: DepDataPropertyBaseArgument<V> | undefined,
    depThis: DepDataObject,
    depIndexService: DepIdentityService
  ) {
    this.objectPropertyIdentifier = depIndexService.createPropertyIdentity(depThis.objectIdentifier, args?.name ?? '');
    this.equal = args?.equal ?? Object.is;
    this.depDataService = depThis.depDataService;
    this.valueVersion = 0;
    this.depThis = depThis;
    depThis.subscription.add(this);
  }

  initilize(scope: DepDataServiceExecutionScope): void {
    if (this.listDependencySink != null) {
      for (const dep of this.listDependencySink) {
        dep.initilize(scope)
      }
    }
  }

  unsubscribe(): void {
    // HERE
    if (this.listDependencySink != null) {
      for (const dep of this.listDependencySink) {
        dep.destroy();
      }
    }
    if (this.listDependencySource != null) {
      if (0 < this.listDependencySource.length) {
        this.depDataService.log(ReportLogLevel.warn, this.objectPropertyIdentifier, "after unsubscribe remaining listDependencySource", 0, { listDependencySource: this.listDependencySource.length });
      }
    }
    this._closed = true;
  }
  private _closed: boolean = false;
  public get closed(): boolean {
    return this._closed;
  }

  // this is the source
  public listDependencySource: (IDepDataDependencySource[] | undefined) = undefined;
  // this is the sink
  public listDependencySink: (IDepDataDependencySink<V>[] | undefined) = undefined;
  public listEffect: (IDepDataEffect[] | undefined) = undefined;

  withSource<TS>(args: DepDataSourceArguments<TS, V>): this {
    const depDataSource = new DepDataDependency<V, TS>(
      args,
      this,
      this.depThis
    );
    if (this.listDependencySink == null) {
      this.listDependencySink = [depDataSource];
    } else {
      this.listDependencySink.push(depDataSource);
    }
    return this;
  }

  withSideEffect(
    args: DepDataEffectArguments<V>
  ): this {
    const depDataSource = new DepDataEffect<V>(args, this);
    if (this.listEffect == null) {
      this.listEffect = [depDataSource];
    } else {
      this.listEffect.push(depDataSource);
    }
    return this;
  }

  addDependencySource(source: IDepDataDependencySource): void {
    if (this.listDependencySource == null) {
      this.listDependencySource = [source];
    } else {
      this.listDependencySource.push(source);
    }
  }

  removeDependencySource(source: IDepDataDependencySource): void {
    if (this.listDependencySource != null) {
      const index = this.listDependencySource.findIndex(item => Object.is(item, source));
      if (0 <= index) {
        this.listDependencySource.splice(index, 1);
      }
    }
  }

  isDirty: boolean = false;
  setDirty(): boolean {
    if (this.isDirty) {
      return false;
    } else {
      this.isDirty = true;
      return true;
    }
  }

  // public getIsDirty(): boolean {
  //   return false;
  // }

  getDirtyWeight(maxlevel: number): number {
    if (this.isDirty) {
      if (this.listDependencySource == null) {
        return 0;
      } else {
        let result = 1;
        if (0 < maxlevel) {
          for (const dep of this.listDependencySource) {
            result += dep.getDirtyWeight(maxlevel - 1);
          }
        }
        return result;
      }
    } else {
      return -1;
    }
  }

  //cleanup(scope: DepDataServiceExecutionScope): void {}

  resetDirty() {
    let nextIsDirty = false;
    if (this.listDependencySink != null) {
      for (const dep of this.listDependencySink) {
        if (dep.getDirtyWeight(0) <= 0) {
          // clean
        } else {
          nextIsDirty = true;
        }
      }
    }
    this.isDirty = nextIsDirty;
  }

  getValue(): V {
    throw new Error("DepDataPropertyBase.getValue is abstract");
  }
  setValue(value: V, scope?: DepDataServiceExecutionScope): void {
    throw new Error("DepDataPropertyBase.setValue is abstract");
  }
}

export class DepDataPropertyValue<V> extends DepDataPropertyBase<V> {
  public value: V;

  constructor(
    args: DepDataPropertyValueArgument<V>,
    depThis: DepDataObject,
    depIdentityService: DepIdentityService
  ) {
    super(args, depThis, depIdentityService);
    this.value = args.initialValue;
  }

  override getValue(): V {
    return this.value;
  }

  override setValue(value: V, scope?: DepDataServiceExecutionScope): void {
    if (this.equal(value, this.value)) {
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyValue.setValue: equal quick exit", scope?.scopeIndex ?? 0, { value: value });
      return;
    } else {
      const listDepDataDependencySource = this.listDependencySource;
      const listEffect = this.listEffect

      if ((0 === (listDepDataDependencySource?.length ?? 0))
        && (0 === (listEffect?.length ?? 0))
      ) {
        const scopeIndex = (scope?.scopeIndex) ?? (this.depThis.depIdentityService.nextScopeIndex());
        this.valueVersion = scopeIndex;
        this.value = value;
      } else {
        const transaction = scope ?? this.depDataService.startScope();
        this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyValue.setValue: set value", transaction.scopeIndex, { value: value });
        this.valueVersion = transaction.scopeIndex;
        this.value = value;

        if (listDepDataDependencySource != null) {
          for (const depDataSource of listDepDataDependencySource) {
            depDataSource.notifyValueChanged(transaction);
          }
        }

        if (listEffect != null) {
          for (const effect of listEffect) {
            transaction.addEffect(effect);
          }
        }

        if (scope == null) {
          this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyValue.setValue: commit-ing", transaction.scopeIndex, { value: value });
          transaction.commit();
        }
      }
    }
  }
}

export class DepDataPropertyWrapSignal<V> extends DepDataPropertyBase<V> {
  signal: Signal<V>;
  value: V;
  private effectRef: EffectRef | undefined;
  constructor(
    signal: Signal<V>,
    args: DepDataPropertyWrapSignalArgument<V> | undefined,
    depThis: DepDataObject,
    depIdentityService: DepIdentityService
  ) {
    super(args, depThis, depIdentityService);
    this.signal = signal;
    {
      let valueS: V = undefined as any;
      untracked(() => { valueS = signal(); });
      this.value = valueS;
    }

    this.effectRef = effect(() => {
      const valueS = signal();
      const scope = this._setValueScope;
      if (scope == null) {
        if (this.equal(this.value, valueS)) {
          this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from signal: equal quick exit", 0, { value: valueS });
          return;
        } else {
          this.value = valueS;
          this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from signal", 0, { value: valueS });
          this.setValueFromSignal(valueS, scope);
        }
      } else {
        this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from setValue", scope.scopeIndex, { value: valueS });
        this._setValueScope = undefined;
      }
    });
  }

  override unsubscribe(): void {
    this.effectRef?.destroy();
    this.effectRef = undefined;
    super.unsubscribe();
  }

  override getValue(): V {
    return this.value;
  }

  override setValue(value: V, scope?: DepDataServiceExecutionScope): void {
    if (this.equal(this.value, value)) {
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValue: equal quick exit", scope?.scopeIndex ?? 0, { value: value });
      return;
    } else {
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValue to signal", scope?.scopeIndex ?? 0, { value: value });      
      this.setValueFromSignal(value, scope);
    }
  }


  private _setValueScope: DepDataServiceExecutionScope | undefined;
  private setValueFromSignal(value: V, scope?: DepDataServiceExecutionScope): void {
    const listDepDataDependencySource = this.listDependencySource;
    const listEffect = this.listEffect

    if ((0 === (listDepDataDependencySource?.length ?? 0))
      && (0 === (listEffect?.length ?? 0))
    ) {
      const scopeIndex = (scope?.scopeIndex) ?? (this.depThis.depIdentityService.nextScopeIndex());
      this.valueVersion = scopeIndex;
      this.value = value;
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: set value - with DependencySource", scopeIndex, { value: value });
    } else {
      const transaction = scope ?? this.depDataService.startScope();
      this.valueVersion = transaction.scopeIndex;
      this.value = value;
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: set value - with DependencySource", transaction.scopeIndex, { value: value });

      if (listDepDataDependencySource != null) {
        for (const depDataSource of listDepDataDependencySource) {
          depDataSource.notifyValueChanged(transaction);
        }
      }

      if (listEffect != null) {
        for (const effect of listEffect) {
          transaction.addEffect(effect);
        }
      }

      if (scope == null) {
        this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: commit-ing", transaction.scopeIndex, { value: value });
        transaction.commit();
      }
    }
  }
}

export class DepDataPropertyWrapWritableSignal<V> extends DepDataPropertyBase<V> {
  signal: WritableSignal<V>;
  value: V;
  private effectRef: EffectRef | undefined;
  constructor(
    signal: WritableSignal<V>,
    args: DepDataPropertyWrapSignalArgument<V> | undefined,
    depThis: DepDataObject,
    depIdentityService: DepIdentityService
  ) {
    super(args, depThis, depIdentityService);
    this.signal = signal;
    {
      let valueS: V = undefined as any;
      untracked(() => { valueS = signal(); });
      this.value = valueS;
    }

    this.effectRef = effect(() => {
      const valueS = signal();
      const scope = this._setValueScope;
      if (scope == null) {
        if (this.equal(this.value, valueS)) {
          this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from signal: equal quick exit", 0, { value: valueS });
          return;
        } else {
          this.value = valueS;
          this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from signal", 0, { value: valueS });
          this.setValueFromSignal(valueS, scope);
        }
      } else {
        this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.effect from setValue", scope.scopeIndex, { value: valueS });
        this._setValueScope = undefined;
      }
    });
  }

  override unsubscribe(): void {
    this.effectRef?.destroy();
    this.effectRef = undefined;
    super.unsubscribe();
  }

  override getValue(): V {
    return this.value;
  }

  override setValue(value: V, scope?: DepDataServiceExecutionScope): void {
    if (this.equal(this.value, value)) {
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValue: equal quick exit", scope?.scopeIndex ?? 0, { value: value });
      return;
    } else {
      const transaction = scope ?? this.depDataService.startScope();
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValue to signal", transaction.scopeIndex, { value: value });
      this._setValueScope = transaction;
      this.signal.set(value);
      if (this._setValueScope == null) {
        // should not be as I understand...
      } else {
        this.setValueFromSignal(value, scope);
        //this._setValueScope = undefined;
      }
      if (scope == null) {
        transaction.commit();
      }
    }
  }

  private _setValueScope: DepDataServiceExecutionScope | undefined;
  private setValueFromSignal(value: V, scope?: DepDataServiceExecutionScope): void {
    const listDepDataDependencySource = this.listDependencySource;
    const listEffect = this.listEffect

    if ((0 === (listDepDataDependencySource?.length ?? 0))
      && (0 === (listEffect?.length ?? 0))
    ) {
      const scopeIndex = (scope?.scopeIndex) ?? (this.depThis.depIdentityService.nextScopeIndex());
      this.valueVersion = scopeIndex;
      this.value = value;
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: set value - with DependencySource", scopeIndex, { value: value });
    } else {
      const transaction = scope ?? this.depDataService.startScope();
      this.valueVersion = transaction.scopeIndex;
      this.value = value;
      this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: set value - with DependencySource", transaction.scopeIndex, { value: value });

      if (listDepDataDependencySource != null) {
        for (const depDataSource of listDepDataDependencySource) {
          depDataSource.notifyValueChanged(transaction);
        }
      }

      if (listEffect != null) {
        for (const effect of listEffect) {
          transaction.addEffect(effect);
        }
      }

      if (scope == null) {
        this.depDataService.log(ReportLogLevel.trace, this.objectPropertyIdentifier, "DepDataPropertyWrapSignal.setValueFromSignal: commit-ing", transaction.scopeIndex, { value: value });
        transaction.commit();
      }
    }
  }
}

// DepDataDependency
export interface IDepDataDependencySource {
  setDependencyDirty(): boolean;
  notifyValueChanged(scope: DepDataServiceExecutionScope): void;
  getDirtyWeight(maxlevel: number): number;
}

export interface IDepDataDependencySink<V> {
  initilize(scope: DepDataServiceExecutionScope): void;
  destroy(): void;
  getDirtyWeight(maxlevel: number): number;
}

export interface IDepDataDependencyCleanup {
  getDirtyWeight(maxlevel: number): number;
  cleanup(scope: DepDataServiceExecutionScope): void;
}

export class DepDataDependency<V, TS> implements IDepDataDependencySink<V>, IDepDataDependencySource, IDepDataDependencyCleanup {
  sourceDependency: DepDataPropertySourceDependencyProperty<TS>;
  sourceTransform: SourceTransform<TS, V>;

  constructor(
    args: DepDataSourceArguments<TS, V>,
    public targetProperty: IDepDataProperty<V>,
    public depThis: DepDataObject
  ) {
    const sourceDependency: DepDataPropertySourceDependencyProperty<TS> = {} as any;
    for (const key in args.sourceDependency) {
      const dep = args.sourceDependency[key];
      if (isSignal(dep)) {
        const depSignal = dep as Signal<TS[typeof key]>;
        this.depThis.createPropertyWrapWritableSignal(depSignal, {
          name: `${this.targetProperty.objectPropertyIdentifier.fullName}-${key}`
        })
        // depThis.createPropertyWrapSignal
        // const depDataProperty=new DepDataPropertyF<TS[typeof key]>();
        // sourceDependency[key]=depDataProperty;
      } else if ("function" === typeof dep.getValue) {
        const depDataProperty = dep as IDepDataProperty<TS[typeof key]>;
        sourceDependency[key] = depDataProperty;
      } else {
        throw new Error(`unexpected ${dep}`);
      }
    }
    this.sourceDependency = sourceDependency;
    this.sourceTransform = args.sourceTransform;
  }

  initilize(scope: DepDataServiceExecutionScope): void {
    for (const key in this.sourceDependency) {
      const depProp = this.sourceDependency[key];
      depProp.addDependencySource(this);
    }
    this.cleanup(scope);
  }

  destroy() {
    for (const key in this.sourceDependency) {
      const dep = this.sourceDependency[key];
      dep.removeDependencySource(this);
    }
  }

  notifyValueChanged(scope: DepDataServiceExecutionScope): void {
    this.targetProperty.setDirty();
    const keysLength = Object.keys(this.sourceDependency).length;
    if (1 === keysLength) {
      this.setDependencyDirty();
      this.cleanup(scope);
      return;
    } else {
      if (this.setDependencyDirty()) {
        scope.addCleanup(this);
      }
    }
  }

  isDependencyDirty: boolean = false;
  setDependencyDirty(): boolean {
    if (this.isDependencyDirty) {
      return false;
    } else {
      this.isDependencyDirty = true;
      return true;
    }
  }
  getDirtyWeight(maxlevel: number): number {
    if (this.isDependencyDirty) {
      let result = 1;
      if (0 < maxlevel) {
        for (const key in this.sourceDependency) {
          const dep = this.sourceDependency[key];
          result += dep.getDirtyWeight(maxlevel - 1);
        }
      }
      return result;
    } else {
      return -1;
    }
  }

  cleanup(scope: DepDataServiceExecutionScope): void {
    const sourceValue: DepDataPropertySourceValue<TS> = {} as any;
    for (const key in this.sourceDependency) {
      const dep = this.sourceDependency[key];
      sourceValue[key] = dep.getValue();
    }
    const currentValue = this.targetProperty.getValue();
    const nextValue = this.sourceTransform(sourceValue, currentValue, scope);
    this.targetProperty.setValue(nextValue, scope)
    this.isDependencyDirty = false;
    this.targetProperty.resetDirty();
  }
}

export class DepDataEffect<V> implements IDepDataEffect {
  sideEffect: (value: V) => void;
  readonly depProperty: IDepDataProperty<V>;
  readonly animationFrame: boolean;
  //readonly depDataService: DepDataService;
  constructor(
    args: DepDataEffectArguments<V>,
    depProperty: IDepDataProperty<V>
  ) {
    this.sideEffect = args.sideEffect;
    this.animationFrame = args.animationFrame ?? false;
    this.depProperty = depProperty;
  }

  execute(): void {
    if (this.animationFrame) {
      window.requestAnimationFrame(() => {
        const value = this.depProperty.getValue();
        this.sideEffect(value);
      });
    } else {
      const value = this.depProperty.getValue();
      this.sideEffect(value);
    }
  }
}

export class DepDataServiceExecutionScope {
  scopeIndex: number;
  readonly objectIdentifier: ObjectIdentifier;

  constructor(
    public depDataService: DepDataService,
    private depIndexService: DepIdentityService
  ) {
    this.scopeIndex = depIndexService.nextScopeIndex();
    this.objectIdentifier = this.depIndexService.createScopeIdentity(this.scopeIndex);
  }

  commit() {
    this.depDataService.log(ReportLogLevel.trace, this.objectIdentifier, "commit", this.scopeIndex, this.listCleanup?.length);

    if (this.listCleanup != null) {
      while (0 < this.listCleanup.length) {
        const cleanup = this.getNextCleanup();
        if (cleanup == null) { continue; }
        cleanup.cleanup(this);
      }
    }

    if (this.listEffect != null) {
      const listEffect = this.listEffect;
      this.listEffect = undefined;
      for (const effect of listEffect) {
        effect.execute();
      }
    }

    this.depDataService.removeScope(this);
  }


  listCleanup: (IDepDataDependencyCleanup[] | undefined) = undefined;
  addCleanup(todo: IDepDataDependencyCleanup) {
    if (this.listCleanup == null) {
      this.listCleanup = [todo];
    } else {
      this.listCleanup.push(todo);
    }
  }
  getNextCleanup() {
    if (this.listCleanup == null) { return undefined; }
    if (0 === this.listCleanup.length) { return undefined; }
    if (1 === this.listCleanup.length) { return this.listCleanup.pop(); }
    let index = 0;
    let nextIndex = 0;
    let nextDirtyWeight = -1;
    while (index < this.listCleanup.length) {
      const cleanup = this.listCleanup[index];
      const dirtyWeight = cleanup.getDirtyWeight(2);
      if (dirtyWeight < 0) {
        this.listCleanup.splice(index, 1);
        return cleanup;
      }
      if (nextDirtyWeight == -1) {
        nextDirtyWeight = dirtyWeight;
        nextIndex = index;
        index++;
        continue;
      }
      if (dirtyWeight < nextDirtyWeight) {
        nextDirtyWeight = dirtyWeight;
        nextIndex = index;
        index++;
        continue;
      }
      {
        index++;
        continue;
      }
    }
    if (0 <= nextIndex && nextIndex < this.listCleanup.length) {
      const cleanup = this.listCleanup.splice(nextIndex, 1)[0];
      return cleanup;
    }

    return this.listCleanup.pop();
  }

  listEffect: (IDepDataEffect[] | undefined) = undefined;
  addEffect(effect: IDepDataEffect) {
    if (this.listEffect == null) {
      this.listEffect = [effect];
    } else {
      this.listEffect.push(effect)
    }
  }
}
