import { AfterViewInit, Component, computed, effect, ElementRef, EnvironmentInjector, HostListener, inject, input, OnDestroy, OnInit, signal, ViewChild, ViewContainerRef } from '@angular/core';
import { CdkMenu, CdkMenuItem, CdkContextMenuTrigger, CdkMenuTrigger } from '@angular/cdk/menu';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { Subscription, combineLatest, tap } from 'rxjs';
import { LucideAngularModule, Funnel, FunnelX, Eye, EyeOff, GripVertical, Menu } from 'lucide-angular';

import { DataService } from '@app/Utility/data-service';
import { getVisualHeader } from '@app/Utility/propertyHeaderUtility';
import { emptyHeaderAndLogLine, emptyLogLineTimeRangeDuration, epoch0, epoch01RangeDuration, epoch1, getEffectiveRange, HeaderAndLogLine, LogLineTimeRangeDuration, setTimeRangeDurationIfChanged, setTimeRangeIfChanged, TimeRangeDuration, TimeRangeOrNull } from '@app/Utility/time-range';
import { getLogLineTimestampValue, LogLine, LogLineValue, PropertyHeader, TraceInformation } from '../Api';

import { TimeScaleComponent } from './time-scale/time-scale.component';
import { DepDataService } from '@app/Utility/dep-data.service';
import { LogTimeDataService } from '@app/Utility/log-time-data.service';
import { openToolWindow } from '@app/tool-window/tool-window';
import { FilterComponent } from '@app/filter/filter.component';

export type LogLineValueWithHeader = LogLineValue & { header: PropertyHeader };

export type DisplayTraceInformation = {
  traceId: string;
  spanId: string;
  parentSpanId: string;
  listLogLineId: number[];
  logLineFirst: LogLine | null;
  logLineLast: LogLine | null;
  xStart: number;
  xFinish: number;
};

@Component({
  selector: 'app-log-view',
  standalone: true,
  imports: [
    ScrollingModule,
    CdkDrag,
    CdkDropList,
    CdkContextMenuTrigger,
    CdkMenu,
    CdkMenuItem,
    LucideAngularModule,
    TimeScaleComponent
  ],
  templateUrl: './log-view.component.html',
  styleUrl: './log-view.component.scss',
})
export class LogViewComponent implements OnInit, AfterViewInit, OnDestroy {
  // Icons
  public readonly Funnel = Funnel;
  public readonly FunnelX = FunnelX;
  public readonly Eye = Eye;
  public readonly EyeOff = EyeOff;
  public readonly GripVertical = GripVertical;
  public readonly Menu = Menu;

  // Services
  public readonly subscription = new Subscription();
  public readonly dataService = inject(DataService);
  public readonly logTimeDataService = inject(LogTimeDataService);
  public readonly depDataService = inject(DepDataService);
  public readonly depDataPropertyInitializer = this.depDataService.createInitializer();
  private readonly viewContainerRef = inject(ViewContainerRef);
  private readonly environmentInjector = inject(EnvironmentInjector);

  // Virtual scroll viewport reference
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // State - Headers
  public readonly listAllHeader = this.depDataService.createProperty({
    name: 'LogView_listAllHeader',
    initialValue: [] as PropertyHeader[],
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      listAllHeader: this.dataService.listAllHeader.dependencyPublic()
    },
    sourceTransform: ({ listAllHeader }) => listAllHeader,
    depDataPropertyInitializer: this.depDataPropertyInitializer
  });

  public readonly listAllHeader$ = this.listAllHeader.asObserable();
  public readonly listCurrentHeader = this.depDataService.createProperty({
    name: 'LogView_listCurrentHeader',
    initialValue: [] as PropertyHeader[],
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      listAllHeader: this.listAllHeader.dependencyInner()
    },
    sourceTransform: ({ listAllHeader }) => getVisualHeader(listAllHeader),
    depDataPropertyInitializer: this.depDataPropertyInitializer
  }
  );

  // State - Log lines
  public readonly dataComplete = this.depDataService.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataComplete',
    initialValue: emptyLogLineTimeRangeDuration,
    subscription: this.subscription,
  }).withSourceIdentity(
    this.logTimeDataService.dataComplete.dependencyPublic(),
    this.depDataPropertyInitializer);
  public readonly $dataComplete = this.dataComplete.asSignal();

  public readonly dataZoom = this.depDataService.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataZoom',
    initialValue: emptyLogLineTimeRangeDuration,
    subscription: this.subscription,
  }).withSourceIdentity(
    this.logTimeDataService.dataZoom.dependencyPublic(),
    this.depDataPropertyInitializer);

  public readonly dataTimeFiltered = this.depDataService.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataTimeFiltered',
    initialValue: emptyLogLineTimeRangeDuration,
    subscription: this.subscription,
  }).withSourceIdentity(
    this.logTimeDataService.dataTimeFiltered.dependencyPublic(),
    this.depDataPropertyInitializer);
  public readonly $dataTimeFiltered = this.dataTimeFiltered.asSignal();

  public readonly dataFilteredCondition = this.depDataService.createProperty<LogLineTimeRangeDuration>({
    name: 'LogView_dataFilteredCondition',
    initialValue: emptyLogLineTimeRangeDuration,
    sideEffect: {
      fn: (value) => {
        this.onScrollIndexChange();
      },
      requestAnimationFrame: true
    },
    subscription: this.subscription,
  }).withSourceIdentity(
    this.logTimeDataService.dataFilteredCondition.dependencyPublic(),
    this.depDataPropertyInitializer);
  public readonly $dataFilteredCondition = this.dataFilteredCondition.asSignal();

  // State - Selection & Highlighting
  public readonly $selectedLogLineId = signal<number | null>(null);
  public readonly selectedLogLineIdProp = this.depDataService.createProperty<number | null>({
    name: 'LogView_selectedLogLineId',
    initialValue: null,
    input: { input: this.$selectedLogLineId },
    compare: (a, b) => (a === b),
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      selectedLogLineId: this.logTimeDataService.currentLogLineId.dependencyInner()
    },
    sourceTransform:
      (d) => d.selectedLogLineId,
    depDataPropertyInitializer: this.depDataPropertyInitializer,
  });

  public readonly highlightedLogLineId = this.depDataService.createProperty<number|null>({
    name: 'LogView_highlightedLogLineId',
    initialValue: null,
    compare: (a, b) => (a === b),
    //input: { input: this.$highlightedLogLineId },
    subscription: this.subscription,
  });
  public readonly $highlightedLogLineId = this.highlightedLogLineId.asSignal();
  
  public readonly selectedLogLine = this.depDataService.createProperty({
    name: 'LogView_selectedLogLine',
    initialValue: null as (LogLine | null),
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency: {
        selectedLogLineId: this.selectedLogLineIdProp.dependencyInner(),
        dataFilteredCondition: this.dataFilteredCondition.dependencyInner()
      },
      sourceTransform:
        ({ selectedLogLineId, dataFilteredCondition }) => {
          if (selectedLogLineId == null) { return null; }
          const result = dataFilteredCondition.listLogLine
            .find(item => item.id === selectedLogLineId)
            ?? null;
          return result;
        },
      depDataPropertyInitializer: this.depDataPropertyInitializer
    });
  public readonly $selectedLogLine = this.selectedLogLine.asSignal();

  public readonly headerId = this.depDataService.createProperty<PropertyHeader | undefined>({
    name: 'LogView_headerId',
    initialValue: undefined,
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      listAllHeader: this.listAllHeader.dependencyInner()
    },
    sourceTransform:
      (d) => d.listAllHeader.find((item) => (item.name === "id")),
    depDataPropertyInitializer: this.depDataPropertyInitializer
  });
  public readonly $headerId = signal<PropertyHeader | undefined>(undefined);

  // State - Time ranges
  public readonly rangeZoom = this.depDataService.createProperty<TimeRangeDuration>({
    name: 'LogView_rangeZoom',
    initialValue: epoch01RangeDuration,
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency: {
        rangeZoom: this.logTimeDataService.rangeZoom.dependencyInner()
      },
      sourceTransform:
        (d) => d.rangeZoom,
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );

  public readonly rangeFilter = this.depDataService.createProperty<TimeRangeDuration>({
    name: 'LogView_rangeFilter',
    initialValue: epoch01RangeDuration,
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency: {
        rangeFilter: this.logTimeDataService.rangeFilter.dependencyInner()
      },
      sourceTransform:
        (d) => d.rangeFilter,
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );

  public readonly gridHeaderWidth = this.depDataService.createProperty({
    name: 'LogView_gridHeaderWidth',
    initialValue: 100,
    compare: (a, b) => a === b,
    subscription: this.subscription,
  }).withSource(
    {
      sourceDependency: {
        dataFilteredCondition: this.dataFilteredCondition.dependencyInner(),
      },
      sourceTransform:
        ({ dataFilteredCondition }) => {
          const oldGridHeaderWidth: number = this.gridHeaderWidth.getValue();
          if (0 === dataFilteredCondition.listLogLine.length) {
            return oldGridHeaderWidth;
          }

          if (dataFilteredCondition.listVisualHeader.length < 2) {
            return oldGridHeaderWidth;
          }

          let nextWidth = 150;
          for (const header of dataFilteredCondition.listVisualHeader) {
            if (header.width < 2) {
              header.width = this.measureHeader(header, dataFilteredCondition.listLogLine);
              console.log("header", header);
            }
            nextWidth += header.width;
          }
          return nextWidth;
        },
      depDataPropertyInitializer: this.depDataPropertyInitializer
    }
  );
  public readonly $gridHeaderWidth = this.gridHeaderWidth.asSignal();

  // State - Visible range in viewport (for timescale sync)
  public readonly visibleRange = signal<TimeRangeOrNull | null>(null);

  // Row height for virtual scroll
  public readonly rowHeight = 32;

  // Resize state
  private resizeState: {
    headerId: string;
    startX: number;
    startWidth: number;
    overlay: HTMLDivElement;
    subscription: Subscription;
  } | null = null;
  // private overlay: HTMLDivElement | null = null;
  //
  @ViewChild('logView2Container', { static: true }) logView2Container!: ElementRef<HTMLDivElement>;
  constructor() {
    this.depDataPropertyInitializer.execute();
  }

  ngOnInit(): void {
    window.requestAnimationFrame(() => {
      this.setupSubscriptions();
    });
  }

  ngAfterViewInit(): void {
    if (!this.viewport) { return; }
    this.updateViewBox();

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
  }

  public readonly displayWidth = this.depDataService.createProperty<number>({
    name: 'LogView_displayWidth',
    initialValue: 0,
    subscription: this.subscription,
  });

  public readonly $displayWidth = this.displayWidth.asSignal();

  @HostListener('window:resize')
  onResize(): void {
    this.updateViewBox();
  }

  private updateViewBox(): void {
    const width = this.logView2Container.nativeElement.clientWidth;
    this.displayWidth.setValue(width);
  }

  public readonly detailsHeaderContent = this.depDataService.createProperty<LogLineValueWithHeader[]>({
    name: 'LogView_detailsHeaderContent',
    initialValue: [] as LogLineValueWithHeader[],
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      selectedLogLine: this.selectedLogLine.dependencyInner()
    },
    sourceTransform:
      (d) => {
        if (d.selectedLogLine == null) { return []; }
        return this.getDetailsHeaderContent(d.selectedLogLine);
      },
    depDataPropertyInitializer: this.depDataPropertyInitializer
  });
  public readonly $detailsHeaderContent = this.detailsHeaderContent.asSignal();

  public readonly detailsBodyContent = this.depDataService.createProperty<LogLineValueWithHeader[]>({
    name: 'LogView_detailsBodyContent',
    initialValue: [] as LogLineValueWithHeader[],
    subscription: this.subscription,
  }).withSource({
    sourceDependency: {
      selectedLogLine: this.selectedLogLine.dependencyInner()
    },
    sourceTransform:
      (d) => {
        if (d.selectedLogLine == null) { return []; }
        return this.getDetailsBodyContent(d.selectedLogLine);
      },
    depDataPropertyInitializer: this.depDataPropertyInitializer
  });
  public readonly $detailsBodyContent = this.detailsBodyContent.asSignal();

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
    return logLine.id === this.$selectedLogLineId();
  }

  isHighlighted(logLine: LogLine): boolean {
    return logLine.id === this.$highlightedLogLineId();
  }

  trackByLogLine(index: number, logLine: LogLine): number {
    return logLine.id;
  }

  // Event handlers
  onSelectLogLine(logLine: LogLine): void {
    this.$selectedLogLineId.set(logLine.id);
    this.logTimeDataService.currentLogLineId.setValue(logLine.id);
    const dataFilteredCondition = this.dataFilteredCondition.getValue();
    const index = dataFilteredCondition.listLogLine.findIndex(l => l.id === logLine.id);
    if (index < 0) { return; }
    this.viewport.scrollToIndex(index);
  }

  onHighlightLogLine(logLine: LogLine | null): void {
    this.highlightedLogLineId.setValue(logLine?.id ?? null);
  }

  onScrollIndexChange(): void {
    if (!this.viewport) {
      console.log("onScrollIndexChange-viewport", this.viewport);
      return;
    }
    const { start, end } = this.viewport.getRenderedRange()
    const { listLogLine } = this.dataFilteredCondition.getValue();
    if (listLogLine.length === 0) { return; }
    const startIndex = Math.max(0, Math.min(start, listLogLine.length - 1));
    const endIndex = Math.max(0, Math.min(end, listLogLine.length - 1));
    const startTS = listLogLine[startIndex].ts;
    const finishTS = listLogLine[endIndex].ts;
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
    const headers = this.listCurrentHeader.getValue();
    moveItemInArray(headers, event.previousIndex, event.currentIndex);
    headers.forEach((h, i) => h.visualHeaderIndex = i);
    this.listCurrentHeader.setValue([...headers]);
  }

  onToggleColumn(header: PropertyHeader): void {
    const allHeaders = this.listAllHeader.getValue();
    const target = allHeaders.find(h => h.id === header.id);
    if (target) {
      target.show = !target.show;
      this.listCurrentHeader.setValue(getVisualHeader(allHeaders));
    }
  }

  onToggleColumnByName(name: string): void {
    const allHeaders = this.listAllHeader.getValue();
    const targets = allHeaders.filter(h => h.name === name);
    const { listLogLine } = this.dataFilteredCondition.getValue();
    for (const header of targets) {
      header.show = !header.show;
      if (header.show) {
        this.measureHeader(header, listLogLine);
      }
    }
    //this.dataService.listAllHeader.setValue(this.listAllHeader.getValue());
    this.dataService.listAllHeader.fireTrigger();
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

    const headers = this.listAllHeader.getValue();
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
    const { listLogLine } = this.dataFilteredCondition.getValue();
    this.measureHeader(header, listLogLine);
    this.updateGridHeader();
    this.dataService.listAllHeader.setValue(this.listAllHeader.getValue());
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

  updateGridHeader() {
    const headers = this.listCurrentHeader.getValue();
    const width = headers.reduce((acc, header) => acc + header.width, 0) + 100 + 50;
    this.gridHeaderWidth.setValue(width);
  }

  public readonly gridHeaderTransform = signal<string | null>(null);
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
    const headers = this.listCurrentHeader.getValue();
    this.logTimeDataService.listFilterCondition.setValue(headers.slice());
  }

  // Time diff helper
  getTimeDiff(logLine: LogLine): string {
    const selectedLog = this.$selectedLogLine();
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

  getDetailsHeaderContent(logLine: LogLine): LogLineValueWithHeader[] {
    const iter = logLine.data.values();
    const result: LogLineValueWithHeader[] = [];
    const mapName = this.dataService.mapName;
    for (const value of iter) {
      const header = mapName.get(value.name)!;
      if (header.visualDetailHeaderIndex >= 0) {
        result.push({ ...value, header: header });
      }
    }
    result.sort((a, b) => {
      return a.header.visualDetailHeaderIndex - b.header.visualDetailHeaderIndex;
    });
    return result;
  }

  getDetailsBodyContent(logLine: LogLine): LogLineValueWithHeader[] {
    const iter = logLine.data.values();
    const result: LogLineValueWithHeader[] = [];
    const mapName = this.dataService.mapName;
    for (const value of iter) {
      const header = mapName.get(value.name)!;
      if (header.visualDetailHeaderIndex < 0) {
        result.push({ ...value, header: header });
      }
    }
    result.sort((a, b) => {
      if ((a.header.visualDetailBodyIndex < 0) && (b.header.visualDetailBodyIndex < 0)) {
        return a.header.name.localeCompare(b.header.name);
      }
      if (a.header.visualDetailBodyIndex < 0) { return 1; }
      if (b.header.visualDetailBodyIndex < 0) { return -1; }
      return a.header.visualDetailBodyIndex - b.header.visualDetailBodyIndex;
    });
    return result;
  }

  // Context menu - id
  //readonly showFilter = signal(false);
  public readonly filterListTraceInformation = signal<DisplayTraceInformation[]>([]);

  @ViewChild(CdkContextMenuTrigger) trigger!: CdkContextMenuTrigger;
  onClickTriggerContextMenuId(event: PointerEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.trigger.open({ x: event.clientX, y: event.clientY });
  }

  // toggleFilter() {
  //   const next = !this.showFilter();
  //   const listDisplayTraceInformation: DisplayTraceInformation[] = [];
  //   if (next) {
  //     const listLogLineAll = this.listLogLineAll.getValue();
  //     const rangeZoom = this.rangeZoom.getValue();
  //     const displayWidth = this.displayWidth.getValue();

  //     for (const traceInformation of this.dataService.mapTraceInformation.values()) {
  //       let logLineFirst: LogLine | null = null;
  //       let logLineLast: LogLine | null = null;
  //       for (let idx = 0; idx < traceInformation.listLogLineId.length; idx++) {
  //         const id = traceInformation.listLogLineId[idx];
  //         //const logLine = binarySearchById(id, listLogLineAll);
  //         const logLine = listLogLineAll.find(l => l.id === id);
  //         if (logLine == null) {
  //           continue;
  //         } else {
  //           logLineFirst = logLine;
  //           break;
  //         }
  //       }
  //       for (let idx = traceInformation.listLogLineId.length - 1; 0 <= idx; idx++) {
  //         const id = traceInformation.listLogLineId[idx];
  //         //const logLine = binarySearchById(id, listLogLineAll);
  //         const logLine = listLogLineAll.find(l => l.id === id);
  //         if (logLine == null) {
  //           continue;
  //         } else {
  //           logLineLast = logLine;
  //           break;
  //         }
  //       }
  //       let xStart = 0;
  //       let xFinish = 0;
  //       if (rangeZoom.start && rangeZoom.finish && rangeZoom.duration && logLineFirst?.ts && logLineLast?.ts) {
  //         /*
  //         const durationZoomMillis = rangeZoom.duration.toMillis();
  //         const startMillis = Duration.between(rangeZoom.start, logLineFirst.ts).toMillis();
  //         const finishMillis = Duration.between(logLineLast.ts, rangeZoom.finish).toMillis();
  //         xStart = displayWidth * (startMillis / durationZoomMillis);
  //         xFinish = displayWidth * (finishMillis / durationZoomMillis);
  //         */
  //         xStart = this.calcPositionX(logLineFirst.ts, rangeZoom, displayWidth);
  //         xFinish = this.calcPositionX(logLineLast.ts, rangeZoom, displayWidth);
  //       }
  //       const displayTraceInformation:DisplayTraceInformation={
  //         traceId: traceInformation.traceId,
  //         spanId: traceInformation.spanId,
  //         parentSpanId: traceInformation.parentSpanId,
  //         listLogLineId: traceInformation.listLogLineId,
  //         logLineFirst: logLineFirst,
  //         logLineLast: logLineLast,
  //         xStart: xStart,
  //         xFinish: xFinish,
  //       };
  //       listDisplayTraceInformation.push(displayTraceInformation);
  //     }
  //   }
  //   this.filterListTraceInformation.set(listDisplayTraceInformation);
  //   this.showFilter.set(next);
  // }

  showAllFieldsInDetails() {
    const allHeaders = this.listAllHeader.getValue();
    const selectedLogLine: LogLine = { id: 0, ts: null, data: new Map<string, LogLineValue>(), traceInformation: null };
    for (const header of allHeaders) {
      selectedLogLine.data.set(header.name, { name: header.name, typeValue: header.typeValue, value: null! });
    }
    this.detailsHeaderContent.setValue(this.getDetailsHeaderContent(selectedLogLine));
    this.detailsBodyContent.setValue(this.getDetailsBodyContent(selectedLogLine));
  }

  openFilterDialog() {
    /*
    const dialogRef = this.dialog.open(FilterDialogComponent, {
      width: '500px',
      data: { filterAst: this.logTimeDataService.filterAst.getValue() }
    });
    */
   openToolWindow(this.viewContainerRef, this.environmentInjector, "Filter", FilterComponent);
  }

  resetRange() {
    const rangeComplete = this.logTimeDataService.rangeComplete.getValue();
    setTimeRangeDurationIfChanged(this.logTimeDataService.rangeZoom, rangeComplete);
    setTimeRangeDurationIfChanged(this.logTimeDataService.rangeFilter, rangeComplete);
  }

  dropDetailsContent(event: CdkDragDrop<LogLineValueWithHeader[]>) {
    console.log("dropDetailsContent", event);
    /*
    TODO: allow reordering of the columns and moving between header and body
    adjust this.getDetailsHeaderContent and this.getDetailsBodyContent
     if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
    }
    */
    return true;
  }

  private calcPositionX(value: ZonedDateTime, rangeZoom: TimeRangeDuration, displayWidth: number): number {
    const durationMillis = rangeZoom.duration.toMillis();
    if (durationMillis <= 0) return 15;
    const currentMillis = Duration.between(rangeZoom.start, value).toMillis();
    const width = displayWidth - 30;
    return 15 + ((currentMillis / durationMillis) * width);
  }

  selectTrace(displayTraceInformation: DisplayTraceInformation) {
    if (displayTraceInformation.listLogLineId.length === 0) { return; }
    this.$selectedLogLineId.set(displayTraceInformation.listLogLineId[0]);
  }
}
