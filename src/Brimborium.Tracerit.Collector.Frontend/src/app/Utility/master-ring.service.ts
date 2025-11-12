import { Injectable } from '@angular/core';
import { MasterRingSubject } from "./MasterRingSubject";
import { DependecyRingSubject as DependecyRingSubject } from "./DependecyRingSubject";
import { Subscription } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class MasterRingService {
  public readonly ring$ = new MasterRingSubject(100, 'ring$');
  public readonly graph: Graph = new Graph();

  constructor() {
    this.ring$.graph = this.graph;
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
        },
        complete: () => {
          result.complete();
        },
        error: (err: any) => {
          result.error(err);
        }
      })
    );
    return result;
  }
}

export class GraphNode {
  public readonly name: string;
  constructor(
    subject: DependecyRingSubject
  ) {
    this.name = subject.name;
  }
  toString() {
    return this.name;
  }
}

export class GraphEdge {
  public readonly key: string;
  constructor(
    from: DependecyRingSubject,
    to: DependecyRingSubject
  ) {
    this.key = `${from.name} -> ${to.name}`;
  }
  toString() {
    return this.key;
  }
}

export class Graph {
  public readonly nodes = new Map<string, GraphNode>();
  public readonly edges = new Map<string, GraphEdge>();
  constructor() {
  }
  addSubject(subject: DependecyRingSubject) {
    let node = this.nodes.get(subject.name);
    if (undefined === node) {
      node = new GraphNode(subject);
      this.nodes.set(node.name, node);
    }
  }

  addSubscription(from: DependecyRingSubject, to: DependecyRingSubject) {
    let fromNode = this.nodes.get(from.name);
    if (undefined === fromNode) {
      fromNode = new GraphNode(from);
      this.nodes.set(from.name, fromNode);
    }
    let toNode = this.nodes.get(to.name);
    if (undefined === toNode) {
      toNode = new GraphNode(to);
      this.nodes.set(to.name, toNode);
    }
    const edge = new GraphEdge(from, to);
    const edgeKey = edge.toString();
    if (!this.edges.has(edgeKey)) {
      this.edges.set(edgeKey, edge);
    }
  }


}