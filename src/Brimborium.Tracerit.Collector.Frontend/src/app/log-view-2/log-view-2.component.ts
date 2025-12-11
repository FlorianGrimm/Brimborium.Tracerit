import { AfterViewInit, Component, computed, effect, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
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
import { epoch0, epoch1, getEffectiveRange, TimeRangeDuration, TimeRangeOrNull } from '../Utility/time-range';
import { getLogLineTimestampValue, LogLine, LogLineValue, PropertyHeader } from '../Api';

import { TimeScale2Component } from './time-scale-2/time-scale-2.component';

const headerContentNames: string[] = [
  "timestamp",
  "logLevel",
  "resource",
  "source",
  "scope",
];
const headerContentNamesPosition = new Map<string, number>([
  ["timestamp", 1],
  ["logLevel", 2],
  ["resource", 3],
  ["source", 4],
  ["scope", 5],
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
export class LogView2Component implements OnInit, AfterViewInit, OnDestroy {
  // Icons
  readonly Funnel = Funnel;
  readonly FunnelX = FunnelX;
  readonly Eye = Eye;
  readonly EyeOff = EyeOff;
  readonly GripVertical = GripVertical;

  // Services
  readonly subscription = new Subscription();
  readonly ring$ = inject(MasterRingService).dependendRing('LogView2-ring$', this.subscription);
  readonly dataService = inject(DataService);
  readonly logTimeDataService = inject(LogTimeDataService);

  // Virtual scroll viewport reference
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // State - Headers
  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 0, 'LogView2_listAllHeader', this.subscription, this.ring$);
  readonly listCurrentHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 0, 'LogView2_listCurrentHeader', this.subscription, this.ring$);

  // State - Log lines
  readonly listLogLineAll$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLineAl', this.subscription, this.ring$);
  readonly listLogLineTimeZoomed$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLineTimeFiltered', this.subscription, this.ring$);
  readonly listLogLineTimeFiltered$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLineTimeFiltered', this.subscription, this.ring$);
  readonly listLogLineFiltered$ = new BehaviorRingSubject<LogLine[]>([], 0, 'LogView2_listLogLineFiltered', this.subscription, this.ring$);

  // State - Selection & Highlighting
  readonly selectedLogLineId = signal<number | null>(null);
  readonly highlightedLogLineId = signal<number | null>(null);
  readonly selectedLogLine = computed(() => {
    const id = this.selectedLogLineId();
    if (id === null) return null;
    return this.listLogLineAll$.getValue().find(l => l.id === id) ?? null;
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
  readonly visibleRange = signal<TimeRangeOrNull | null>(null);

  // Row height for virtual scroll
  readonly rowHeight = 32;

  // Resize state
  private resizeState: {
    headerId: string;
    startX: number;
    startWidth: number;
    overlay: HTMLDivElement;
    subscription: Subscription;
  } | null = null;
  // private overlay: HTMLDivElement | null = null;


  constructor() {
  }
  ngOnInit(): void {
    window.requestAnimationFrame(() => {
      this.setupSubscriptions();
    });
  }
  ngAfterViewInit(): void {
    if (!this.viewport) { return; }

    const onScrollViewport = (ev: Event) => this.onScrollViewport(ev);
    const nativeElement = this.viewport.elementRef.nativeElement;
    nativeElement.addEventListener('scroll', onScrollViewport);
    this.subscription.add(() => { nativeElement.removeEventListener('scroll', onScrollViewport); });
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

    const localSubscription = new Subscription();
    this.subscription.add(localSubscription);
    localSubscription.add(
      combineLatest({
        listCurrentHeader: this.listCurrentHeader$,
        listLogLineFiltered: this.listLogLineFiltered$
      }).subscribe({
        next: ({ listCurrentHeader, listLogLineFiltered }) => {
          if (0 === listLogLineFiltered.length) {
            return;
          }
          if (listCurrentHeader.length < 2) {
            return;
          }
          for (const header of listCurrentHeader) {
            if (header.width < 100) {
              this.measureHeader(header, listLogLineFiltered);
            }
          }
          this.updateGridHeader();
          localSubscription.unsubscribe();
        }
      })

    );

    // Subscribe to log lines
    this.subscription.add(
      this.logTimeDataService.listLogLineAll$.subscribe({
        next: (value) => {
          this.listLogLineAll$.next(value);
        }
      })
    );
    this.subscription.add(
      this.logTimeDataService.listLogLineTimeZoomed$.subscribe({
        next: (value) => {
          this.listLogLineTimeZoomed$.next(value);
        }
      })
    );

    this.subscription.add(
      this.logTimeDataService.listLogLineFilteredCondition$.subscribe({
        next: (value) => {
          this.listLogLineFiltered$.next(value);
          window.requestAnimationFrame(() => {
            this.onScrollIndexChange();
          });
        }
      })
    );

    // Subscribe to range zoom
    this.subscription.add(
      this.logTimeDataService.rangeZoom$.subscribe({
        next: (value) => {
          this.rangeZoom$.next(value);
        }
      })
    );

    // Subscribe to range filter
    this.subscription.add(
      this.logTimeDataService.rangeFilter$.subscribe({
        next: (value) => {
          this.rangeFilter$.next(value);
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
    const listLogLineFiltered = this.listLogLineFiltered$.getValue();
    const index = listLogLineFiltered.findIndex(l => l.id === logLine.id);
    if (index < 0) { return; }
    this.viewport.scrollToIndex(index);
  }

  onHighlightLogLine(logLine: LogLine | null): void {
    this.highlightedLogLineId.set(logLine?.id ?? null);
  }

  onScrollIndexChange(): void {
    if (!this.viewport) {
      console.log("onScrollIndexChange-viewport", this.viewport);
      return;
    }
    const { start, end } = this.viewport.getRenderedRange()
    const listLogLineFiltered = this.listLogLineFiltered$.getValue();
    const startIndex = Math.max(0, Math.min(start, listLogLineFiltered.length - 1));
    const endIndex = Math.max(0, Math.min(end, listLogLineFiltered.length - 1));
    const startTS = listLogLineFiltered[startIndex].ts;
    const finishTS = listLogLineFiltered[endIndex].ts;
    //console.log("onScrollIndexChange-viewport", startTS?.toString(), finishTS?.toString());
    this.visibleRange.set(
      {
        start: startTS,
        finish: finishTS
      }
    );
  }

  // Column management
  onDropHeader(event: CdkDragDrop<PropertyHeader[]>): void {
    const headers = this.listCurrentHeader$.getValue();
    moveItemInArray(headers, event.previousIndex, event.currentIndex);
    headers.forEach((h, i) => h.visualHeaderIndex = i);
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
    const listLogLine = this.listLogLineFiltered$.getValue();
    for (const header of targets) {
      header.show = !header.show;
      if (header.show) {
        this.measureHeader(header, listLogLine);
      }
    }
    this.dataService.listAllHeader$.next(this.listAllHeader$.getValue());
    this.updateGridHeader();

  }

  // Column resize
  onResizeStart(event: MouseEvent, header: PropertyHeader | undefined): void {
    console.log("onResizeStart", event);
    if (header == null) { return; }
    event.preventDefault();
    event.stopPropagation();
    const element = document.getElementById(`header-${header.id}`);
    if (!element) return;

    const overlay = document.createElement('div');
    const subscription = new Subscription();
    this.resizeState = {
      headerId: header.id,
      startX: event.screenX,
      startWidth: element.clientWidth,
      overlay: overlay,
      subscription: subscription
    };

    Object.assign(overlay.style, {
      position: 'fixed', top: '0', left: '0', width: '100%', height: '100%',
      zIndex: '1000', cursor: 'col-resize', userSelect: 'none'
    });
    overlay.addEventListener('mousemove', this.onResizeMove);
    overlay.addEventListener('mouseup', this.onResizeEnd);
    overlay.addEventListener('mouseleave', this.onResizeEnd);
    document.body.appendChild(overlay);
    subscription.add(() => {
      overlay.removeEventListener('mousemove', this.onResizeMove);
      overlay.removeEventListener('mouseup', this.onResizeEnd);
      overlay.removeEventListener('mouseleave', this.onResizeEnd);
      document.body.removeChild(overlay);
      this.resizeState = null;
    });
  }

  private onResizeMove = (event: MouseEvent): void => {
    if (!this.resizeState) return;
    const diff = event.screenX - this.resizeState.startX;
    const nextWidth = Math.max(50, this.resizeState.startWidth + diff);

    const headers = this.listAllHeader$.getValue();
    const header = headers.find(h => h.id === this.resizeState!.headerId);
    if (header) {
      header.width = nextWidth;
      header.headerCellStyle = { width: `${nextWidth}px` };
      header.dataCellStyle = { width: `${nextWidth}px` };
      this.updateGridHeader();
    }
  };

  private onResizeEnd = (event: MouseEvent): void => {
    this.onResizeMove(event);
    this.cleanupResize();
  };

  private cleanupResize(): void {
    if (this.resizeState != null) {
      this.resizeState.subscription.unsubscribe();
      this.resizeState = null;
    }
  }

  onAutoSize(header: PropertyHeader) {
    this.cleanupResize();
    const listLogLine = this.listLogLineFiltered$.getValue();
    this.measureHeader(header, listLogLine);
    this.updateGridHeader();
    this.dataService.listAllHeader$.next(this.listAllHeader$.getValue());
  }

  private measureCanvas?: HTMLCanvasElement | undefined;
  measureHeader(header: PropertyHeader, listLogLine: LogLine[]) {
    let nextWidth = 100;
    let nextMaxContent = '';
    for (const logLine of listLogLine) {
      const value = logLine.data.get(header.name);
      if (value) {
        const content = this.getContent(logLine, header);
        if (nextMaxContent.length < content.length) { nextMaxContent = content; }
        const width = this.getContent(logLine, header).length * 6;
        if (nextWidth < width) { nextWidth = width; }
      }
    }
    {
      let measureCanvas: HTMLCanvasElement
      if (this.measureCanvas == null) {
        measureCanvas = window.document.createElement('canvas');
        measureCanvas.style.fontFamily = 'monospace';
        measureCanvas.style.fontSize = '12px';
        this.measureCanvas = measureCanvas;
        this.subscription.add(() => {
          if (this.measureCanvas) {
            this.measureCanvas.remove();
            this.measureCanvas = undefined;
          }
        });
      } else {
        measureCanvas = this.measureCanvas;
      }
      const ctxt = measureCanvas.getContext('2d');
      if (ctxt != null) {
        ctxt.font = '12px monospace';
        const width = ctxt.measureText(nextMaxContent).width + 40;
        console.log("onAutoSize", header.name, nextWidth, width);
        if (nextWidth < width) { nextWidth = width; }
      }
    }

    header.width = nextWidth;
    header.headerCellStyle = { width: `${nextWidth}px` };
    header.dataCellStyle = { width: `${nextWidth}px` };
    return nextWidth;
  }

  readonly gridHeaderWidth = signal<number | null>(null);
  updateGridHeader() {
    const headers = this.listCurrentHeader$.getValue();
    const width = headers.reduce((acc, header) => acc + header.width, 0) + 100 + 50;
    this.gridHeaderWidth.set(width);
  }

  readonly gridHeaderTransform = signal<string | null>(null);
  onScrollViewport(ev: Event): void {
    const scrollLeft = (ev.target as (HTMLDivElement | null))?.scrollLeft;
    if (scrollLeft) {
      this.gridHeaderTransform.set(`translateX(${(-scrollLeft)}px)`);
    } else {
      this.gridHeaderTransform.set(null);
    }
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
    const result: (LogLineValue & { header: PropertyHeader })[] = [];
    const mapName = this.dataService.mapName;
    for (const value of iter) {
      if (headerContentNames.includes(value.name)) {
        // ignore
      } else {
        result.push({ ...value, header: mapName.get(value.name)! });
      }
    }
    result.sort((a, b) => {
      let cmp = a.header.visualDetailIndex - b.header.visualDetailIndex;
      if (cmp !== 0) { return cmp; }
      cmp = (a.name).localeCompare(b.name);
      return cmp;
    });
    return result;
  }

}
