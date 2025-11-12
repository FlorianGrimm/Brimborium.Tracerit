import type { MonoTypeOperatorFunction } from 'rxjs';
import { BehaviorSubject, from, Observable, Subscription } from 'rxjs';

export function debounceToggle<T>(
    condition: Observable<boolean>
): MonoTypeOperatorFunction<T> {
    return (source: Observable<T>) => new Observable<T>(
        (destination) => {
            const subscription = new Subscription();
            const state: {
                open: boolean;
                pending: boolean;
                value: T | undefined;
            } = { open: false, pending: false, value: undefined };
            if (condition instanceof BehaviorSubject) {
                state.value = condition.getValue();
            }
            subscription.add(
                from(condition).subscribe({
                    next: (value) => {
                        const opening = !state.open && value;
                        state.open = value;
                        if (opening) {
                            if (state.pending) {
                                state.pending=false;
                                destination.next(state.value!);
                            }
                        }
                    },
                    complete: () => {
                        subscription.unsubscribe();
                    },
                    error: (err: any) => {
                        subscription.unsubscribe();
                    }
                }));
            subscription.add(
                source.subscribe({
                    next: (value) => {
                        if (state.open) {
                            if(state.pending){
                                state.pending=false;
                                state.value=undefined;
                            }
                            destination.next(value);
                        } else {
                            state.pending = true;
                            state.value=value;
                        }
                    },
                    complete: () => {
                        destination.complete();
                        subscription.unsubscribe();
                    },
                    error: (err: any) => {
                        destination.error(err);
                        subscription.unsubscribe();
                    }
                }));
        });
}