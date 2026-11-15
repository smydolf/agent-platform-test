import { test, expect } from '@playwright/test'

test('AC1: GET /orders/{id} for a missing order returns 404 with JSON error body', async ({ request }) => {
  const r = await request.get('/orders/999999')
  expect(r.status()).toBe(404)
  expect(await r.json()).toEqual({ error: 'Order not found' })
})

test('AC2: an existing order is returned as before', async ({ request }) => {
  const r = await request.get('/orders/1')
  expect(r.status()).toBe(200)
  const body = await r.json()
  expect(body.id).toBe(1)
  expect(body).toHaveProperty('total')
  expect(body).toHaveProperty('currency')
})
