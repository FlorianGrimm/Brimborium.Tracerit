import { inject, Injectable } from '@angular/core';
import { MasterRingSubject } from "./MasterRingSubject";
import { Subscription } from 'rxjs';
import { TraceableSubjectGraphService } from './traceable-subject-graph.service';

@Injectable({
  providedIn: 'root',
})
export class MasterRingService {
  public readonly ring$ = new MasterRingSubject(100, 'ring$');
  public readonly graph = inject(TraceableSubjectGraphService).getGraph();// new Graph();

  constructor() {
    this.ring$.graph = this.graph;
    // only for debug remove later
    (window as any)['__graph'] = this.graph;
  }

  public setRing(value: number) {
    for (let currentValue = this.ring$.getValue();
      currentValue <= value;
      currentValue++
    ) {
      this.ring$.next(currentValue);
    }
  }

  public dependendRing(name: string, subscription: Subscription) {
    const result = new MasterRingSubject(1, 'ring$');
    result.graph = this.graph;
    subscription.add(
      this.ring$.subscribe({
        next: (value) => {
          result.next(value);
        }
        // complete: () => {
        //   result.complete();
        // },
        // error: (err: any) => {
        //   result.error(err);
        // }
      })
    );
    return result;
  }
}