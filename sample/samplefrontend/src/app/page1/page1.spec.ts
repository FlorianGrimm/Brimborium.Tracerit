import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Page1 } from './page1';
import { provideZonelessChangeDetection } from '@angular/core';

describe('Page1', () => {
  let component: Page1;
  let fixture: ComponentFixture<Page1>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
      imports: [Page1]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Page1);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
