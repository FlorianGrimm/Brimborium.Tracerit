import {
  LucideAngularModule,
  ChartNoAxesGantt,
  ChevronLeft,
  ChevronRight,
  SquareX,
  Eye,
  EyeOff,
  FileStack,
  Funnel,
  FunnelX,
  GripVertical,
  Menu,
  MessageSquareText,
  Search,
  Spotlight,
  ZoomIn,
  ZoomOut
} from 'lucide-angular';

export class AppIconComponent {
  
  public readonly LogView = MessageSquareText;
  public readonly DirectoryList = FileStack;
  public readonly Close = SquareX;
  public readonly Search = Search;
  public readonly Filter = Funnel;
  public readonly FilterX = FunnelX;
  public readonly Highlight = Spotlight;
  public readonly TraceView = ChartNoAxesGantt;
  public readonly ChevronLeft = ChevronLeft;
  public readonly ChevronRight = ChevronRight;
  public readonly ZoomIn = ZoomIn;
  public readonly ZoomOut = ZoomOut;
  public readonly Eye = Eye;
  public readonly EyeOff = EyeOff;
  public readonly GripVertical = GripVertical;
  public readonly Menu = Menu;
}
