import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AddressService } from './address.service';
import { apiResponseInterceptor } from '../interceptors/api-response.interceptor';

const addressData = {
  id: 'a1',
  label: 'Casa',
  address: 'Av. Reforma 123, Depto 4, Ciudad de México',
  latitude: 19.43,
  longitude: -99.13,
  isDefault: true,
};

const envelope = (data: unknown) => ({ success: true, message: 'ok', data, errors: null });

describe('AddressService', () => {
  let service: AddressService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiResponseInterceptor])),
        provideHttpClientTesting(),
        AddressService,
      ],
    });

    service = TestBed.inject(AddressService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should GET /api/customer/addresses', () => {
    service.getAddresses().subscribe((res) => {
      expect(res).toHaveLength(1);
      expect(res[0].label).toBe('Casa');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/customer/addresses');
    expect(req.request.method).toBe('GET');
    req.flush(envelope([addressData]));
  });

  it('should POST /api/customer/addresses on createAddress', () => {
    service.createAddress({
      label: 'Oficina',
      address: 'Av. Insurgentes 300',
      latitude: 19.41,
      longitude: -99.16,
      isDefault: false,
    }).subscribe((res) => {
      expect(res.id).toBe('a1');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/customer/addresses');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      label: 'Oficina',
      address: 'Av. Insurgentes 300',
      latitude: 19.41,
      longitude: -99.16,
      isDefault: false,
    });
    req.flush(envelope(addressData));
  });

  it('should PUT /api/customer/addresses/:id on updateAddress', () => {
    service.updateAddress('a1', { label: 'Casa 2', address: 'Calle 5' }).subscribe((res) => {
      expect(res.isDefault).toBe(true);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/customer/addresses/a1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ label: 'Casa 2', address: 'Calle 5' });
    req.flush(envelope(addressData));
  });

  it('should DELETE /api/customer/addresses/:id and unwrap data', () => {
    service.deleteAddress('a1').subscribe((res) => {
      expect(res).toBe(true);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/customer/addresses/a1');
    expect(req.request.method).toBe('DELETE');
    req.flush(envelope(true));
  });
});
