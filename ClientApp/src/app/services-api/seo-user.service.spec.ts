import { TestBed } from '@angular/core/testing';

import { SeoUserService } from './seo-user.service';

describe('SeoUserService', () => {
  let service: SeoUserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SeoUserService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
