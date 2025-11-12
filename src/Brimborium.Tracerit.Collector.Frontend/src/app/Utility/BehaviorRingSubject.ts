import { BehaviorSubject, Observable, Subscription } from "rxjs";
import { DependecyRingSubject, DependecyRingSubjectStatic } from "./DependecyRingSubject";
import { MasterRingSubject } from "./MasterRingSubject";

export function createBehaviorRingSubject<T>(
    subscription: Subscription,
    source: (() => Observable<T>),
    initialValue: T,
    ring: number,
    name: string,
    conditionSubject?: MasterRingSubject,
    fnPaused?: (that: BehaviorRingSubject<T>, ring: undefined | MasterRingSubject) => void,
    fnReport?: ((name: string, message: string, value: T | undefined) => void)
): BehaviorRingSubject<T> {
    const result = new BehaviorRingSubject<T>(initialValue, ring, name, subscription, conditionSubject, fnPaused, fnReport);
    const observer = source();
    const localSubscription = new Subscription();
    subscription.add(localSubscription);
    localSubscription.add(
        observer.subscribe({
            next: (value) => {
                result.next(value);
            },
            complete: () => {
                localSubscription.unsubscribe();
            },
            error: () => {
                localSubscription.unsubscribe();
            }
        })
    );
    return result;
}


export class BehaviorRingSubject<T extends Exclude<any, undefined>> extends BehaviorSubject<T> implements DependecyRingSubject {
    private state = {
        enabled: true as boolean,
        currentRing: 0 as number,
        pending: false,
        value: undefined as T | undefined,
    };
    public readonly name: string;
    constructor(
        value: T,
        public readonly  ring: number,
        name: string,
        public readonly subscriptionRing?: Subscription,
        public readonly ring$?: MasterRingSubject,
        private readonly fnPaused?: (that: BehaviorRingSubject<T>, ring: undefined | MasterRingSubject) => void,
        private readonly fnReport?: ((name: string, message: string, value: T | undefined) => void)
    ) {
        super(value);
        const index=++DependecyRingSubjectStatic.InstanceIndex;
        this.name = `${name}-${index}`;
        if (undefined !== subscriptionRing && undefined !== ring$) {
            ring$.graph?.addSubject(this);
            this.state.enabled = true;
            this.state.currentRing = ring$.getValue();
            subscriptionRing.add(
                ring$.subscribe({
                    next: (value) => {
                        this.setCurrentRing(value);
                    },
                    complete: () => {
                        this.setCurrentRing(0);
                    },
                    error: () => {
                        this.setCurrentRing(0);
                    }
                }));
        } 
    }
    setCurrentRing(value: number) {
        this.state.currentRing = value;
        if (this.state.currentRing >= this.ring) {
            if (this.state.pending) {
                this.state.pending = false;

                const nextValue = this.state.value;
                this.state.value = undefined;

                if (undefined !== this.fnReport) {
                    this.fnReport(this.name, "resume", nextValue);
                }
                super.next(nextValue!);
            }
        }
    }

    public override next(value: T): void {
        if (this.state.enabled) {
            if (this.state.currentRing >= this.ring) {
                if (undefined !== this.fnReport) {
                    this.fnReport(this.name, "next", value);
                }
                super.next(value);
            } else {
                this.state.value = value;
                if (this.state.pending) {
                    if (undefined !== this.fnReport) {
                        this.fnReport(this.name, "throttle", value);
                    }
                } else {
                    this.state.pending = true;
                    if (undefined !== this.fnReport) {
                        this.fnReport(this.name, "pause", value);
                    }
                }
                if (undefined !== this.fnPaused) {
                    this.fnPaused(this, this.ring$);
                }
            }
        } else {
            if (undefined !== this.fnReport) {
                this.fnReport(this.name, "next", value);
            }
            super.next(value);
        }
    }

    validateRing(target$: BehaviorRingSubject<any>, name?:string) {
        if (this.ring < target$.ring) {
            // ok
            this.ring$?.graph?.addSubscription(this, target$);
        } else {
            throw new Error(`${this.name}.ring:${this.ring} >= ${target$.name}.ring:${target$.ring} ${name}`);
        }
        return this;
    }

    pipeAndSubscribe<R extends Exclude<any, undefined>>(
        observer: BehaviorRingSubject<R>,
        pipeFn: (subject: BehaviorRingSubject<T>) => Observable<R | undefined>,
        fnNext?: (value: R, observer: BehaviorRingSubject<R>) => void
    ): Subscription {
        this.validateRing(observer);
        const pipe$: Observable<R | undefined> = pipeFn(this);
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