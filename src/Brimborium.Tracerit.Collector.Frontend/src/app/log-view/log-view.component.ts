import { Component, inject } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { LogLine, PropertyHeader } from '../Api';
import { AsyncPipe } from '@angular/common';
import { getVisualHeader } from '../Utility/propertyHeaderUtility';
import { LucideAngularModule, FileStack, ChevronLeft, ChevronRight } from 'lucide-angular';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-log-view',
  imports: [
    AsyncPipe,
    RouterLink,
    LucideAngularModule],
  templateUrl: './log-view.component.html',
  styleUrl: './log-view.component.scss',
})
export class LogViewComponent {
  readonly FileStack = FileStack;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;

  readonly subscription = new Subscription();
  readonly dataService = inject(DataService);
  readonly httpClientService = inject(HttpClientService);
  readonly listAllHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  readonly listCurrentHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  readonly listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  readonly currentLogLine$ = new BehaviorSubject<number | undefined>(undefined);
  readonly error$ = new BehaviorSubject<undefined | string>(undefined);

  constructor() {

    this.subscription.add(
      this.dataService.listLogLine$.subscribe(this.listLogLine$));
    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          this.listAllHeader$.next(value);
          this.listCurrentHeader$.next(getVisualHeader(value));
        }
      }));
  }
  getContent(logLine: LogLine, header: PropertyHeader) : string {
    const property = logLine.data.get(header.name);
    if (undefined === property) { return ""; }
    switch(property.typeValue){
      case 'null': return 'null';
      case 'str': return property.value;
      case 'enum': return property.value;
      case 'uuid': return property.value;
      case 'lvl': return property.value;
      case 'int': return property.value.toString();
      case 'dbl': return property.value.toString();
      case 'bool': return property.value.toString();
      case 'dt': return property.value.toString();
      case 'dto': return property.value.toString();
      case 'dur': return property.value.toString();
      default: return (property as any).value?.toString() ?? "";
    }
    //return property.value?.toString() ?? "";
  }


  setCurrentLogLine(id: number) {
    this.currentLogLine$.next(id);
  }


}
