---
layout: post
title: Controllo Robot via WiFi con Comandi JSON
date: 2026-01-28
description: Test e prove di controllo robot tramite comandi JSON inviati da una sezione di debug dell'app mobile
tags: robotics arduino esp32 json wifi
categories: projects
thumbnail: assets/img/26989_b3a51143.768x768.jpg
---

In questo articolo descrivo alcune prove di controllo di un robot tramite comandi JSON inviati da una sezione di debugging di un'app mobile. L'architettura utilizza due microcontrollori che comunicano tra loro per gestire i comandi.

## Architettura del Sistema

Il sistema è composto da tre componenti principali:

### 1. App Mobile - Sezione Debug
Una sezione di debugging dell'app mobile che permette di inviare comandi JSON di test al robot tramite WiFi. L'interfaccia consente di impartire semplici comandi per verificare il funzionamento dei vari componenti.

<div class="row mt-3">
    <div class="col-sm mt-3 mt-md-0">
        {% include figure.liquid loading="eager" path="assets/img/Screenshot_20260128-222608_Chrome.jpg" class="img-fluid rounded z-depth-1" zoomable=true %}
    </div>
</div>
<div class="caption">
    Screenshot dell'app mobile che mostra l'interfaccia per inviare comandi JSON al robot
</div>

### 2. ESP32 - Gateway WiFi
L'ESP32 agisce come gateway WiFi, ricevendo i comandi JSON dall'app mobile e inoltrandoli tramite comunicazione seriale all'Arduino. Questo permette di separare la gestione della connettività wireless dalla logica di controllo del robot.

### 3. Arduino - Controller Hardware
L'Arduino riceve i comandi dalla seriale e si occupa di controllare direttamente i componenti hardware del robot:
- **Motori**: per il movimento
- **LED RGB**: per la segnalazione visiva
- **Servo motori**: per movimenti precisi
- **Sensori ultrasuoni**: per rilevare ostacoli

<div class="row mt-3">
    <div class="col-sm mt-3 mt-md-0">
        {% include figure.liquid loading="eager" path="assets/img/26989_b3a51143.768x768.jpg" class="img-fluid rounded z-depth-1" zoomable=true %}
    </div>
</div>
<div class="caption">
    Il robot completo con tutti i componenti montati
</div>

## Protocollo Comandi JSON

Il protocollo di comunicazione si basa su messaggi JSON strutturati. Ogni comando contiene un numero identificativo (`N`) e parametri opzionali (`D1`, `D2`, `D3`, `D4`).

### Esempi di Comandi

#### Controllo LED RGB
```json
{"N": 7, "D1": 0, "D2": 255, "D3": 0, "D4": 0}
```
Accende il LED frontale con colore ROSSO (R=255, G=0, B=0).

#### Controllo Servo
```json
{"N": 6, "D1": 1, "D2": 90}
```
Muove il servo motore 1 alla posizione di 90 gradi.

#### Lettura Sensore
```json
{"N": 21, "D1": 2}
```
Richiede la lettura del sensore ultrasuoni con ID 2.

#### Stop di Emergenza
```json
{"N": 100}
```
Ferma immediatamente tutti i motori e le operazioni in corso.

## Tabella Comandi

La seguente tabella riassume tutti i comandi disponibili:

| Comando (N) | Descrizione | Parametri |
|:------------|:------------|:----------|
| `6` | Controllo Servo | `D1`: ID servo (0-n), `D2`: angolo (0-180°) |
| `7` | Controllo LED RGB | `D1`: indice LED, `D2`: valore R (0-255), `D3`: valore G (0-255), `D4`: valore B (0-255) |
| `21` | Lettura sensore ultrasuoni | `D1`: ID sensore |
| `100` | STOP emergenza | Nessun parametro richiesto |

## Osservazioni sui Test

Durante le prove, questa architettura modulare ha dimostrato alcuni vantaggi interessanti:

1. **Separazione delle responsabilità**: L'ESP32 gestisce la connettività WiFi mentre l'Arduino si concentra sul controllo hardware
2. **Flessibilità nei test**: È facile testare nuovi comandi o sensori modificando solo il codice Arduino
3. **Debugging semplificato**: I due microcontrollori possono essere testati indipendentemente
4. **Protocollo estendibile**: Il formato JSON consente di aggiungere facilmente nuovi tipi di comandi per ulteriori test

## Conclusioni

Questi test mostrano come sia possibile controllare un robot utilizzando comandi JSON inviati da una semplice sezione di debug di un'app mobile. L'architettura a due microcontrollori si è rivelata pratica per testare vari componenti e comandi in modo flessibile, utilizzando componenti comuni e un protocollo di comunicazione leggibile e facilmente estendibile.
