import { test, expect } from '@playwright/test'

test('AC1: GET /orders/{id}/total returns 200 and total as a two-decimal string', async ({ request }) => {
  const r1 = await request.get('/orders/1/total')
  expect(r1.status()).toBe(200)
  expect(await r1.json()).toEqual({ id: 1, total: '100.00' })
  const r2 = await request.get('/orders/2/total')
  expect(r2.status()).toBe(200)
  expect(await r2.json()).toEqual({ id: 2, total: '250.50' })
})

test('AC2: GET /orders/{id}/total for a missing order returns 404', async ({ request }) => {
  const r = await request.get('/orders/999999/total')
  expect(r.status()).toBe(404)
})
