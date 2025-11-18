import { inject, Injectable } from '@angular/core';
import { Observable, Observer } from 'rxjs';
import { NamedSubject, TraceableInformation, TraceableSubject } from './TraceableSubject';
import { Graph } from './Graph';

@Injectable({
  providedIn: 'root',
})
export class TraceableSubjectGraphService {
  public readonly graph = new Graph();
  constructor() {
  }
  public getGraph() {
    return this.graph;
  }
}


let state: ({
  enabled: false,
  traceableSubjectGraphService: undefined,
} | {
  enabled: true,
  traceableSubjectGraphService: TraceableSubjectGraphService,
}) & {
  identifierIndex: number;
} = {
  enabled: false,
  traceableSubjectGraphService: undefined,
  identifierIndex: 1,
}

export function enableTraceableSubject(): TraceableSubjectGraphService {
  if (!state.enabled) {
    state = {
      ...state,
      enabled: true,
      traceableSubjectGraphService: inject(TraceableSubjectGraphService),
    };
  }
  return state.traceableSubjectGraphService as TraceableSubjectGraphService;
}

export function getTraceableInformation<T>(subject: Observer<T>|Observable<T>|TraceableSubject, name?: string) {
  let result=(subject as TraceableSubject)._TraceableInformation;
  if (undefined !== result) {
    return result;
  }
  if (undefined === name) {
    name = (subject as NamedSubject).name;
  }
  result = new TraceableInformation(name, ++state.identifierIndex);
  (subject as TraceableSubject)._TraceableInformation = new TraceableInformation(undefined, ++state.identifierIndex);

  state.traceableSubjectGraphService?.graph.addSubject(result);

  return result;
}
export function getExistingTraceableInformation<T>(subject: TraceableSubject|object) {
  return (subject as TraceableSubject)._TraceableInformation;
}