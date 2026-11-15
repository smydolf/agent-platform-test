import { test, expect } from '@playwright/test'

// Seed: order 1 (100.00 USD), order 2 (250.50 EUR), order 3 (75.00, NULL currency).
// Property names are matched case-insensitively: the serializer uses camelCase.
const field = (o: Record<string, unknown>, name: string) =>
  o[Object.keys(o).find((k) => k.toLowerCase() === name.toLowerCase()) ?? name]

test('AC1: GET /orders/{id} for an order with a NULL currency returns 200 and Currency null', async ({ request }) => {
  const r = await request.get('/orders/3')
  expect(r.status()).toBe(200)
  const body = await r.json()
  expect(Object.keys(body).map((k) => k.toLowerCase())).toContain('currency')
  expect(field(body, 'currency')).toBeNull()
})

test('AC2: orders with a currency behave as before', async ({ request }) => {
  const r = await request.get('/orders/1')
  expect(r.status()).toBe(200)
  const body = await r.json()
  expect(field(body, 'id')).toBe(1)
  expect(field(body, 'total')).toBe(100)
  expect(field(body, 'currency')).toBe('USD')

  const r2 = await request.get('/orders/2')
  expect(r2.status()).toBe(200)
  expect(field(await r2.json(), 'currency')).toBe('EUR')
})
