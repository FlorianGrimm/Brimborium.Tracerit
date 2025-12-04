import { Component, computed, effect, inject, OnDestroy, signal, ViewChild } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { Subscription, combineLatest, tap } from 'rxjs';
import { LucideAngularModule, Funnel, FunnelX, Eye, EyeOff, GripVertical } from 'lucide-angular';

import { DataService } from '../Utility/data-service';
import { LogTimeDataService } from '../Utility/log-time-data.service';
import { MasterRingService } from '../Utility/master-ring.service';
import { BehaviorRingSubject } from '../Utility/BehaviorRingSubject';
import { getVisualHeader } from '../Utility/propertyHeaderUtility';
import { epoch0, epoch1, TimeRangeDuration, TimeRangeOrNull } from '../Utility/time-range';
import { getLogLineTimestampValue, LogLine, LogLineValue, PropertyHeader } from '../Api';

import { TimeScale2Component, VisibleRange } from './time-scale-2/time-scale-2.component';

const headerContentNames: string[] = [
  "timestamp",
  "logLevel",
  "source",
  "scope",
];
const headerContentNamesPosition = new Map<string, number>([
  ["timestamp", 1],
  ["logLevel", 2],
  ["source", 3],
  ["scope", 4],
]);

@Component({
  selector: 'app-log-view-2',
  standalone: true,
  imports: [
    AsyncPipe,
    ScrollingModule,
    CdkDropList,
    CdkDrag,
    LucideAngularModule,
    TimeScale2Component
  ],
  templateUrl: './log-view-2.component.html',
  styleUrl: './log-view-2.component.scss',
})
export class LogView2Component implements OnDestroy {
  // Icons
  readonly Funnel = Funnel;
  readonly FunnelX = FunnelX;
  readonly Eye = Eye;
  readonly EyeOff = EyeOff;
  readonly GripVertical = GripVertical;

  // Services
  private readonly subscription = new Subscription();
  private readonly ring$ = inject(MasterRingService).dependendRing('LogView2-ring$', this.subscription);
  private readonly dataService = inject(DataService);
  private readonly logTimeDataService = inject(LogTimeDataService);

  // Virtual scroll viewport reference
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // State - Headers
  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 0, 'LogView2_listAllHeader', this.subscription, this.ring$);
  readonly listCurrentHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 0, 'LogView2_listCurrentHeader', this.subscription, this.ring$);

  // State - Log lines
  readonly listLogLine$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLine', this.subscription, this.ring$);
  readonly listLogLineFiltered$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLineFiltered', this.subscription, this.ring$);

  // State - Selection & Highlighting
  readonly selectedLogLineId = signal<number | null>(null);
  readonly highlightedLogLineId = signal<number | null>(null);
  readonly selectedLogLine = computed(() => {
    const id = this.selectedLogLineId();
    if (id === null) return null;
    return this.listLogLineFiltered$.getValue().find(l => l.id === id) ?? null;
  });
  readonly headerId = signal<PropertyHeader | undefined>(undefined);

  // State - Time ranges
  readonly rangeZoom$ = new BehaviorRingSubject<TimeRangeDuration>(
    { start: epoch0, finish: epoch1, duration: Duration.between(epoch0, epoch1) },
    0, 'LogView2_rangeZoom', this.subscription, this.ring$
  );
  readonly rangeFilter$ = new BehaviorRingSubject<TimeRangeOrNull>(
    { start: null, finish: null },
    0, 'LogView2_rangeFilter', this.subscription, this.ring$
  );

  // State - Visible range in viewport (for timescale sync)
  readonly visibleRange = signal<VisibleRange | null>(null);

  // Row height for virtual scroll
  readonly rowHeight = 32;

  // Resize state
  private resizeState: { headerId: string; startX: number; startWidth: number } | null = null;
  private overlay: HTMLDivElement | null = null;

  constructor() {
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.cleanupResize();
  }

  private setupSubscriptions(): void {
    // Subscribe to headers from data service
    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          const nextValue = value.slice();

          const idHeader = nextValue.find((item) => (item.name === "id"));
          this.headerId.set(idHeader);

          this.listAllHeader$.next(nextValue);
          this.listCurrentHeader$.next(getVisualHeader(nextValue));
        }
      })
    );

    // Subscribe to log lines
    this.subscription.add(
      this.logTimeDataService.listLogLineFilteredTime$.subscribe({
        next: (value) => {
          this.listLogLine$.next(value.slice());
        }
      })
    );

    // Subscribe to range zoom
    this.subscription.add(
      this.logTimeDataService.rangeZoom$.subscribe({
        next: (value) => this.rangeZoom$.next(value)
      })
    );

    // Subscribe to range filter
    this.subscription.add(
      this.logTimeDataService.rangeFilter$.subscribe({
        next: (value) => this.rangeFilter$.next(value)
      })
    );

    // Filter log lines by time range
    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLine$,
        rangeFilter: this.rangeFilter$
      }).subscribe({
        next: ({ listLogLine, rangeFilter }) => {
          const rangeFilterStart = rangeFilter.start ?? epoch0;
          const rangeFilterFinish = rangeFilter.finish ?? epoch1;

          const testStart = (epoch0.compareTo(rangeFilterStart) !== 0);
          const testFinish = (epoch1.compareTo(rangeFilterFinish) !== 0);

          const listFiltered = listLogLine.filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (!ts) return false;
            return (testStart ? (rangeFilterStart.compareTo(ts) <= 0) : true)
              && (testFinish ? (ts.compareTo(rangeFilterFinish) <= 0) : true);
          });
          this.listLogLineFiltered$.next(listFiltered);
        }
      })
    );

    // Sync selection with logTimeDataService
    this.subscription.add(
      this.logTimeDataService.currentLogLineId$.subscribe({
        next: (id) => this.selectedLogLineId.set(id)
      })
    );
  }

  // Template helpers
  getContent(logLine: LogLine, header: PropertyHeader): string {
    const property = logLine.data.get(header.name);
    if (!property) return '';
    switch (property.typeValue) {
      case 'null': return 'null';
      case 'str': return property.value;
      case 'enum': return property.value;
      case 'uuid': return property.value;
      case 'lvl': return property.value;
      case 'int': return property.value.toString();
      case 'dbl': return property.value.toString();
      case 'bool': return property.value.toString();
      case 'dt': return property.value.toString().replace('T', '\r\n');
      case 'dto': return property.value.toString().replace('T', '\r\n');
      case 'dur': return property.value.toString();
      default: return (property as any).value?.toString() ?? '';
    }
  }

  isSelected(logLine: LogLine): boolean {
    return logLine.id === this.selectedLogLineId();
  }

  isHighlighted(logLine: LogLine): boolean {
    return logLine.id === this.highlightedLogLineId();
  }

  trackByLogLine(index: number, logLine: LogLine): number {
    return logLine.id;
  }

  // Event handlers
  onSelectLogLine(logLine: LogLine): void {
    this.selectedLogLineId.set(logLine.id);
    this.logTimeDataService.currentLogLineId$.next(logLine.id);
  }

  onHighlightLogLine(logLine: LogLine | null): void {
    this.highlightedLogLineId.set(logLine?.id ?? null);
  }

  onRangeFilterChange(range: TimeRangeOrNull): void {
    this.rangeFilter$.next(range);
    this.logTimeDataService.rangeFilter$.next({
      start: range.start ?? epoch0,
      finish: range.finish ?? epoch1
    });
  }

  onScrollIndexChange(): void {
    if (!this.viewport) return;
    const start = this.viewport.getRenderedRange().start;
    const end = this.viewport.getRenderedRange().end;
    this.visibleRange.set({ startIndex: start, endIndex: end });
  }

  // Column management
  onDropHeader(event: CdkDragDrop<PropertyHeader[]>): void {
    const headers = this.listCurrentHeader$.getValue();
    moveItemInArray(headers, event.previousIndex, event.currentIndex);
    headers.forEach((h, i) => h.visualIndex = i);
    this.listCurrentHeader$.next([...headers]);
  }

  onToggleColumn(header: PropertyHeader): void {
    const allHeaders = this.listAllHeader$.getValue();
    const target = allHeaders.find(h => h.id === header.id);
    if (target) {
      target.show = !target.show;
      this.listCurrentHeader$.next(getVisualHeader(allHeaders));
    }
  }

  onToggleColumnByName(name: string): void {
    const allHeaders = this.listAllHeader$.getValue();
    const targets = allHeaders.filter(h => h.name === name);
    targets.forEach(h => h.show = !h.show);
    this.listCurrentHeader$.next(getVisualHeader(allHeaders));
  }

  // Column resize
  onResizeStart(event: MouseEvent, header: PropertyHeader | undefined): void {
    if (header == null) { return; }
    event.preventDefault();
    event.stopPropagation();
    const element = document.getElementById(`header-${header.id}`);
    if (!element) return;

    this.resizeState = {
      headerId: header.id,
      startX: event.screenX,
      startWidth: element.clientWidth
    };

    this.overlay = document.createElement('div');
    Object.assign(this.overlay.style, {
      position: 'fixed', top: '0', left: '0', width: '100%', height: '100%',
      zIndex: '1000', cursor: 'col-resize', userSelect: 'none'
    });
    this.overlay.addEventListener('mousemove', this.onResizeMove);
    this.overlay.addEventListener('mouseup', this.onResizeEnd);
    document.body.appendChild(this.overlay);
  }

  private onResizeMove = (event: MouseEvent): void => {
    if (!this.resizeState) return;
    const diff = event.screenX - this.resizeState.startX;
    const newWidth = Math.max(50, this.resizeState.startWidth + diff);

    const headers = this.listAllHeader$.getValue();
    const header = headers.find(h => h.id === this.resizeState!.headerId);
    if (header) {
      header.headerCellStyle = { width: `${newWidth}px` };
      header.dataCellStyle = { width: `${newWidth}px` };
    }
  };

  private onResizeEnd = (event: MouseEvent): void => {
    this.onResizeMove(event);
    this.cleanupResize();
  };

  private cleanupResize(): void {
    if (this.overlay) {
      this.overlay.removeEventListener('mousemove', this.onResizeMove);
      this.overlay.removeEventListener('mouseup', this.onResizeEnd);
      document.body.removeChild(this.overlay);
      this.overlay = null;
    }
    this.resizeState = null;
  }

  // Filter management
  addFilter(logLine: LogLine, header: PropertyHeader): void {
    const value = logLine.data.get(header.name);
    if (value) {
      header.filter = { ...value };
      this.triggerFilter();
    }
  }

  removeFilter(header: PropertyHeader): void {
    header.filter = undefined;
    this.triggerFilter();
  }

  private filterCounter = 0;
  private triggerFilter(): void {
    this.filterCounter++;
    const headers = this.listCurrentHeader$.getValue();
    this.logTimeDataService.listFilterCondition$.next(headers.slice());
  }

  // Time diff helper
  getTimeDiff(logLine: LogLine): string {
    const selectedLog = this.selectedLogLine();
    if (!selectedLog) return '';

    const selectedTs = getLogLineTimestampValue(selectedLog);
    const currentTs = getLogLineTimestampValue(logLine);
    if (!selectedTs || !currentTs) return '';

    const dur = Duration.between(selectedTs, currentTs);
    const millis = dur.toMillis();
    if (Math.abs(millis) < 2000) return `${millis.toFixed(0)}ms`;
    const seconds = dur.seconds();
    if (Math.abs(seconds) < 100) return `${seconds.toFixed(1)}s`;
    const minutes = seconds / 60;
    if (Math.abs(minutes) < 240) return `${minutes.toFixed(1)}m`;
    return dur.toString();
  }

  getHeaderContent(logLine: LogLine) {
    const iter = logLine.data.values();
    const result: LogLineValue[] = [];
    for (const value of iter) {
      if (headerContentNames.includes(value.name)) {
        result.push(value);
      } else {
        // ignore
      }
    }
    result.sort((a, b) => {
      return (headerContentNamesPosition.get(a.name) ?? 0) - (headerContentNamesPosition.get(b.name) ?? 0);
    });
    return result;
  }
  getDetailsContent(logLine: LogLine) {
    const iter = logLine.data.values();
    const result: LogLineValue[] = [];
    for (const value of iter) {
      if (headerContentNames.includes(value.name)) {
        // ignore
      } else {
        result.push(value);
      }
    }
    return result;
  }

}
