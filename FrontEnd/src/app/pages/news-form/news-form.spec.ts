import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NewsForm } from './news-form';

describe('NewsForm', () => {
  let component: NewsForm;
  let fixture: ComponentFixture<NewsForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewsForm],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(NewsForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
