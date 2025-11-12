import { combineLatest, defer, distinctUntilKeyChanged, filter, map, Observable, ObservableInput, ObservedValueOf, Subscriber, Subscription, tap } from "rxjs";
import { DependecyRingSubject, DependecyRingSubjectStatic } from "./DependecyRingSubject";
import { MasterRingSubject } from "./MasterRingSubject";
import { BehaviorRingSubject } from "./BehaviorRingSubject";


export function combineLatestRingSubject<T extends Record<string, ObservableInput<any> & DependecyRingSubject>>(
    sourcesObject: T,
    ring: number,
    name: string,
    ring$?: MasterRingSubject,
    // private readonly fnPaused?: (that: CombineLatestRingSubject<T>, ring: undefined | MasterRingSubject) => void,
    fnReport?: ((name: string, message: string, value: T | undefined) => void)
    // Observable<{ [K in keyof T]: ObservedValueOf<T[K]> }>{
): CombineLatestRingSubject<T> {
    return new CombineLatestRingSubject(sourcesObject, ring, name, ring$);
}

export class CombineLatestRingSubject<T extends Record<string, ObservableInput<any> & DependecyRingSubject>>
    implements DependecyRingSubject {
    public readonly ring: number;
    public readonly name: string;
    private readonly sourcesObject: T;

    constructor(
        sourcesObject: T,
        ring: number,
        name: string,
        public readonly ring$?: MasterRingSubject,
        // private readonly fnPaused?: (that: CombineLatestRingSubject<T>, ring: undefined | MasterRingSubject) => void,
        private readonly fnReport?: ((name: string, message: string, value: T | undefined) => void)
    ) {
        const index = ++DependecyRingSubjectStatic.InstanceIndex;
        this.sourcesObject = sourcesObject;
        this.ring = ring;
        this.name = `${name}-${index}`;
        for (const key in sourcesObject) {
            if (!Object.hasOwn(sourcesObject, key)) continue;

            const element = sourcesObject[key];
            element.validateRing(this, key);
        }
    }

    validateRing(target$: DependecyRingSubject<any>, name?:string) {
        if (this.ring < target$.ring) {
            // ok
            this.ring$?.graph?.addSubscription(this, target$);
        } else {
            throw new Error(`${this.name}.ring:${this.ring} >= ${target$.name}.ring:${target$.ring} ${name}`);
        }
        return this;
    }

    combineLatest(): Observable<{ [K in keyof T]: ObservedValueOf<T[K]>; }> {
        const result = combineLatest(this.sourcesObject)
        if (undefined !== this.fnReport) {
            return result.pipe(
                
                tap({
                    subscribe: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "subscribe", undefined);
                        }
                    },
                    next: (value) => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "next", value);
                        }
                    },
                    complete: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "complete", undefined);
                        }
                    },
                    error: (err) => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "error", err);
                        }
                    },
                    unsubscribe: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "unsubscribe", undefined);
                        }
                    }
                })
            );
        } else {
            return result;
        }
    }

    pipeAndSubscribe<R extends Exclude<any, undefined>>(
        observer: BehaviorRingSubject<R>,
        pipeFn: (subject: Observable<{ [K in keyof T]: ObservedValueOf<T[K]> }>) => Observable<R | undefined>,
        fnNext?: (value: R, observer: BehaviorRingSubject<R>) => void
    ): Subscription {
        this.validateRing(observer, observer.name);
        const combineLatest$ = this.combineLatest();
        const pipe$: Observable<R | undefined> = pipeFn(combineLatest$);
        return pipe$.subscribe({
            next: (value) => {
                if (undefined !== value) {
                    if (undefined !== fnNext) {
                        fnNext(value, observer);
                    } else {
                        observer.next(value);
                    }
                }
            }
            // complete: () => {
            //     observer.complete();
            // },
            // error: (err) => {
            //     observer.error(err);
            // }
        });
    }

    static defaultLog(name: string, message: string, value: any | undefined) {
        console.log(name, message, value);
    }

}
