import { test, expect } from '@playwright/test'

test('AC1: GET /health returns 200 with JSON {"status":"ok"} as application/json', async ({ request }) => {
  const r = await request.get('/health')
  expect(r.status()).toBe(200)
  expect(r.headers()['content-type']).toContain('application/json')
  expect(await r.json()).toEqual({ status: 'ok' })
})

test('AC2: other endpoints do not change (GET /orders/1 still returns the order JSON)', async ({ request }) => {
  const r = await request.get('/orders/1')
  expect(r.status()).toBe(200)
  expect(r.headers()['content-type']).toContain('application/json')
  expect(await r.json()).toEqual({ id: 1, total: 100.0, currency: 'USD' })
})
