import { FileSizePipe } from './file-size.pipe';

describe('FileSizePipe', () => {
  it('1 B', () => {
    const pipe = new FileSizePipe();
    expect(pipe.transform(1)).toBe("1 B");
  })
  it('1.00 KB', () => {
    const pipe = new FileSizePipe();
    expect(pipe.transform(1024)).toBe("1.00 KB");
  })
  
  it('1.01 KB with dot', () => {
    const pipe = new FileSizePipe();
    expect(pipe.transform(1024*1.01)).toBe("1.01 KB");
  })
  
  it('1.00 MB', () => {
    const pipe = new FileSizePipe();
    expect(pipe.transform(1024*1024)).toBe("1.00 MB");
  })
  
  it('1.23 MB', () => {
    const pipe = new FileSizePipe();
    expect(pipe.transform(1024*1024*1.23)).toBe("1.23 MB");
  })
});
