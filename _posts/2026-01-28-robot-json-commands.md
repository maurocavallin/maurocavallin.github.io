---
layout: post
title: Controllo Robot via WiFi con Comandi JSON
date: 2026-01-28
description: Architettura a due microcontrollori per controllare un robot tramite comandi JSON inviati da un'app mobile
tags: robotics arduino esp32 json wifi
categories: projects
thumbnail: assets/img/26989_b3a51143.768x768.jpg
---

In questo progetto, ho sviluppato un sistema per controllare un robot tramite comandi JSON inviati da un'app mobile. Il sistema utilizza un'architettura a due microcontrollori per massimizzare la flessibilità e la modularità.

## Architettura del Sistema

Il sistema è composto da tre componenti principali:

### 1. App Mobile
Un'applicazione nativa che permette di inviare comandi JSON al robot tramite WiFi. L'interfaccia è semplice e intuitiva, permettendo di controllare tutti gli aspetti del robot in tempo reale.

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

## Vantaggi dell'Architettura

Questa architettura modulare offre diversi vantaggi:

1. **Separazione delle responsabilità**: L'ESP32 gestisce la connettività WiFi mentre l'Arduino si concentra sul controllo hardware
2. **Flessibilità**: È facile aggiungere nuovi comandi o sensori modificando solo il codice Arduino
3. **Scalabilità**: Il protocollo JSON può essere esteso facilmente per supportare nuovi tipi di comandi
4. **Debug semplificato**: I due microcontrollori possono essere testati indipendentemente
5. **Riutilizzabilità**: La stessa architettura può essere adattata per diversi tipi di robot

## Conclusioni

Questo progetto dimostra come sia possibile realizzare un sistema di controllo robotico modulare e flessibile utilizzando componenti comuni e un protocollo di comunicazione basato su JSON. L'architettura a due microcontrollori permette di sfruttare al meglio le caratteristiche di ciascun componente, ottenendo un sistema robusto e facilmente estensibile.
