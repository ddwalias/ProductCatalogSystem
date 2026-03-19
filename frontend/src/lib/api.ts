import type {
  CategoryTreeItem,
  CategoryUpdatePayload,
  CategoryWritePayload,
  PagedResult,
  ProductAutocompleteItem,
  ProductDetail,
  ProductListItem,
  ProductUpdatePayload,
  ProductWritePayload,
} from './types';
import { logFrontendWarning, reportFrontendError } from './telemetry';

export class ApiError extends Error {
  readonly status: number;
  readonly errors: Record<string, string[]> | null;

  constructor(status: number, message: string, errors: Record<string, string[]> | null = null) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}

interface ProblemDetailsResponse {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

function buildQuery(params: Record<string, string | number | null | undefined>): string {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== '') {
      searchParams.set(key, String(value));
    }
  });

  const query = searchParams.toString();
  return query.length > 0 ? `?${query}` : '';
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;

  try {
    response = await fetch(path, {
      headers: {
        Accept: 'application/json',
        ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
        ...init?.headers,
      },
      ...init,
    });
  } catch (error) {
    reportFrontendError('API request failed before receiving a response', error, {
      'http.request.method': init?.method ?? 'GET',
      'url.full': path,
    });
    throw error;
  }

  if (!response.ok) {
    const fallbackMessage = `Request failed with status ${response.status}.`;
    logFrontendWarning('API request returned a non-success status code', {
      'http.request.method': init?.method ?? 'GET',
      'http.response.status_code': response.status,
      'url.full': path,
    });

    try {
      const problem = (await response.json()) as ProblemDetailsResponse;
      throw new ApiError(
        response.status,
        problem.detail ?? problem.title ?? fallbackMessage,
        problem.errors ?? null,
      );
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }

      throw new ApiError(response.status, fallbackMessage);
    }
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export function getCategories(): Promise<CategoryTreeItem[]> {
  return request<CategoryTreeItem[]>('/api/categories');
}

export function getProducts(params: {
  query?: string;
  cursor?: string | null;
  pageSize: number;
  categoryId?: number | null;
  priceFrom?: number | null;
  priceTo?: number | null;
  sortBy?: string;
  sortDir?: string;
}): Promise<PagedResult<ProductListItem>> {
  return request<PagedResult<ProductListItem>>(
    `/api/products${buildQuery(params)}`,
  );
}

export function getCategoryProducts(id: number, params: {
  cursor?: string | null;
  pageSize: number;
  query?: string;
  priceFrom?: number | null;
  priceTo?: number | null;
  sortBy?: string;
  sortDir?: string;
}): Promise<PagedResult<ProductListItem>> {
  return request<PagedResult<ProductListItem>>(
    `/api/categories/${id}/products${buildQuery(params)}`,
  );
}

export function getProductAutocomplete(params: {
  query: string;
  limit?: number;
}): Promise<ProductAutocompleteItem[]> {
  return request<ProductAutocompleteItem[]>(
    `/api/products/search${buildQuery(params)}`,
  );
}

export function getProduct(id: number, version?: number): Promise<ProductDetail> {
  return request<ProductDetail>(`/api/products/${id}${buildQuery({ version })}`);
}

export function createProduct(payload: ProductWritePayload): Promise<ProductDetail> {
  return request<ProductDetail>('/api/products', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function updateProduct(id: number, payload: ProductUpdatePayload): Promise<ProductDetail> {
  return request<ProductDetail>(`/api/products/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export function deleteProduct(id: number): Promise<void> {
  return request<void>(`/api/products/${id}`, { method: 'DELETE' });
}

export function createCategory(payload: CategoryWritePayload): Promise<CategoryTreeItem> {
  return request<CategoryTreeItem>('/api/categories', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function updateCategory(id: number, payload: CategoryUpdatePayload): Promise<CategoryTreeItem> {
  return request<CategoryTreeItem>(`/api/categories/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export async function getHealth(): Promise<string> {
  const response = await fetch('/api/health', {
    headers: {
      Accept: 'text/plain',
    },
  });

  if (!response.ok) {
    throw new ApiError(response.status, `Health check failed with status ${response.status}.`);
  }

  return response.text();
}
