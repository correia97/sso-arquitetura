import { TestBed } from '@angular/core/testing';

import { RequestInteceptorService } from './request-inteceptor.service';

describe('RequestInteceptorService', () => {
  let service: RequestInteceptorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RequestInteceptorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
