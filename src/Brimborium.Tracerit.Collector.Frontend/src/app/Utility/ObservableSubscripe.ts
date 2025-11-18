import { BehaviorSubject, Observable, subscribeOn, Subscription } from "rxjs";

export function createObserableSubscripe<T>(
    args: {
        obs: Observable<T>
        subscribtion: Subscription,
        immediate: boolean
    }
): ObserableSubscripe<T> {
    return new ObserableSubscripe<T>(args);
}
export class ObserableSubscripe<T> {
    globalSubscribtion: Subscription;
    localsubscribtion: Subscription;
    obs: Observable<T>;
    constructor(args: {
        obs: Observable<T>
        subscribtion: Subscription,
        immediate: boolean
    }) {
        this.obs = args.obs;
        this.globalSubscribtion = args.subscribtion;
        if (args.immediate){
            this.localsubscribtion=this.subscribe();
        } else {
            this.localsubscribtion=new Subscription();
        }        
    }

    subscribe(): Subscription {
        const result = this.obs.subscribe({
            next: (value) => {
            },
            complete: () => {
            },
            error: (err) => {
            }
        });
        this.localsubscribtion=result;
        this.globalSubscribtion.add(result);
        return result;
    }
    unsubscribe() {
        this.localsubscribtion.unsubscribe();
    }
}